// Example: Services/SearchIndexingService.cs (New or Modified File)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Models.Flashcards;
using Microsoft.EntityFrameworkCore;
using backend.Models.LearningEnvironment;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql; 
using OpenSearch.Client;
using OpenSearch.Net;




namespace backend.Services
{
    public class SearchIndexingService 
    {
        private readonly IOpenSearchClient _openSearchClient;
        private readonly DataContext _dbContext;
        private readonly ILogger<SearchIndexingService> _logger;
        private const string IndexName = "borgertinget-search";

        public SearchIndexingService(
            IOpenSearchClient openSearchClient,
            DataContext dbContext,
            ILogger<SearchIndexingService> logger
        )
        {
            _openSearchClient =
                openSearchClient ?? throw new ArgumentNullException(nameof(openSearchClient));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task RunFullIndexAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Starting full indexing from PostgreSQL to OpenSearch index '{IndexName}'...",
                IndexName
            );

            var bulkRequest = new BulkRequest(IndexName);
            var operations = new List<IBulkOperation>();
            int aktorCount = 0;
            int flashcardCount = 0;
            int partyCount = 0;
            int pagesCount = 0;
            const int batchSize = 300;

            try
            {
                // --- Index Aktors ---
                _logger.LogInformation("Fetching Aktors from database...");
                var aktors = await _dbContext
                    .Aktor.AsNoTracking()
                    .Where(a => a.typeid == 5) 
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Mapping {Count} Aktors for indexing...", aktors.Count);
                foreach (var aktor in aktors)
                {
                    var searchDoc = MapAktorToSearchDocument(aktor);
                    operations.Add(
                        new BulkIndexOperation<SearchDocument>(searchDoc) { Id = searchDoc.Id }
                    );
                    aktorCount++;

                    if (operations.Count >= batchSize)
                    {
                        await SendBulkRequestAsync(operations, cancellationToken);
                        operations.Clear();
                        _logger.LogInformation(
                            "Indexed batch of {BatchSize} documents...",
                            batchSize
                        );
                    }
                }
                _logger.LogInformation("Finished mapping Aktors.");

                // --- Index Flashcards ---
                _logger.LogInformation("Fetching Flashcards from database...");
                var flashcards = await _dbContext
                    .Flashcards.AsNoTracking()
                    .Include(f => f.FlashcardCollection) // Include collection for title
                    .ToListAsync(cancellationToken);

                _logger.LogInformation(
                    "Mapping {Count} Flashcards for indexing...",
                    flashcards.Count
                );
                foreach (var flashcard in flashcards)
                {
                    var searchDoc = MapFlashcardToSearchDocument(flashcard);
                    operations.Add(
                        new BulkIndexOperation<SearchDocument>(searchDoc) { Id = searchDoc.Id }
                    );
                    flashcardCount++;

                    if (operations.Count >= batchSize)
                    {
                        await SendBulkRequestAsync(operations, cancellationToken);
                        operations.Clear();
                        _logger.LogInformation(
                            "Indexed batch of {BatchSize} documents...",
                            batchSize
                        );
                    }
                }
                _logger.LogInformation("Finished mapping Flashcards.");

                // Index Party
                _logger.LogInformation("Fetching Parties");
                var parties = await _dbContext.Party.AsNoTracking().ToListAsync();

                foreach ( var party in parties){
                    var searchDoc = MapPartyToSearchDocument(party);
                    operations.Add(
                        new BulkIndexOperation<SearchDocument>(searchDoc) {Id = searchDoc.Id}
                    );
                    partyCount++;
                    if (operations.Count >= batchSize){
                        await SendBulkRequestAsync(operations, cancellationToken);
                        operations.Clear();
                        _logger.LogInformation(
                            "Indexed batch of {BatchSize} documents...", batchSize
                        );
                    }
                }
                _logger.LogInformation("Finished mapping Parties");

                //Index Learning env
                _logger.LogInformation("Fetching pages");
                var pages = await _dbContext.Pages.AsNoTracking().ToListAsync();

                foreach(var page in pages){
                    var searchDoc = MapPageToSearchDocument(page);
                    operations.Add(
                        new BulkIndexOperation<SearchDocument>(searchDoc) {Id = searchDoc.Id}
                    );
                    pagesCount++;
                    if (operations.Count >= batchSize){
                        await SendBulkRequestAsync(operations, cancellationToken);
                        operations.Clear();
                    }
                }
                _logger.LogInformation("Finished mapping pages");

                // Send any remaining documents
                if (operations.Count > 0)
                {
                    await SendBulkRequestAsync(operations, cancellationToken);
                    _logger.LogInformation(
                        "Indexed final batch of {Count} documents.",
                        operations.Count
                    );
                }

                _logger.LogInformation(
                    "Finished indexing. Total Aktors: {AktorCount}, Total Flashcards: {FlashcardCount}, Total Parties: {partyCount}",
                    aktorCount,
                    flashcardCount,
                    partyCount
                );
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Indexing operation was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during indexing process.");
                throw;
            }
        }

        // Helper to map Aktor to the unified SearchDocument
        private SearchDocument MapAktorToSearchDocument(Aktor aktor)
        {
            // Combine relevant text fields for searchable content
            var contentParts = new List<string?>
            {
                aktor.navn,
                aktor.Party,
                aktor.PartyShortname,
                aktor.MinisterTitel,
                aktor.FunctionFormattedTitle,
                aktor.PositionsOfTrust,
                aktor.Sex,
                aktor.Born,
            };
            contentParts.AddRange(aktor.Spokesmen ?? Enumerable.Empty<string>());
            contentParts.AddRange(aktor.Ministers ?? Enumerable.Empty<string>());
            contentParts.AddRange(aktor.ParliamentaryPositionsOfTrust ?? Enumerable.Empty<string>());
            contentParts.AddRange(aktor.Constituencies ?? Enumerable.Empty<string>());
            contentParts.AddRange(aktor.Educations ?? Enumerable.Empty<string>()); 
            contentParts.AddRange(aktor.Occupations ?? Enumerable.Empty<string>());

            
            var suggestInputs = new List<string>();
            if (!string.IsNullOrWhiteSpace(aktor.navn)) suggestInputs.Add(aktor.navn);
            if (!string.IsNullOrWhiteSpace(aktor.Party)) suggestInputs.Add(aktor.Party);
            if (!string.IsNullOrWhiteSpace(aktor.PartyShortname)) suggestInputs.Add(aktor.PartyShortname);

            return new SearchDocument
            {
                Id = $"aktor-{aktor.Id}",
                DataType = "Aktor",
                Title = aktor.navn,
                Content = string.Join(
                    " | ",
                    contentParts.Where(s => !string.IsNullOrWhiteSpace(s))
                ),
                LastUpdated = DateTime.UtcNow,

                // Aktor specific
                AktorName = aktor.navn,
                Party = aktor.Party,
                PartyShortname = aktor.PartyShortname,
                MinisterTitle = aktor.MinisterTitel,
                Constituencies = aktor.Constituencies,
                Suggest = suggestInputs.Any() ? new CompletionField { Input = suggestInputs } : null
            };
        }

        // Helper to map Flashcard to the unified SearchDocument
        private SearchDocument MapFlashcardToSearchDocument(Flashcard flashcard)
        {
            var contentParts = new List<string?> { flashcard.FrontText, flashcard.BackText };

            string? title =
                flashcard.FrontContentType == FlashcardContentType.Text
                    ? flashcard.FrontText
                    : $"Flashcard from '{flashcard.FlashcardCollection?.Title ?? "Unknown Collection"}'";
            var suggestInputs = new List<string?>();
            if (!string.IsNullOrWhiteSpace(title)) suggestInputs.Add(title);
            if (!string.IsNullOrWhiteSpace(flashcard.FrontText)) suggestInputs.Add(flashcard.FrontText);
            if (!string.IsNullOrWhiteSpace(flashcard.BackText)) suggestInputs.Add(flashcard.BackText);
            if (!string.IsNullOrWhiteSpace(flashcard.FlashcardCollection?.Title)) suggestInputs.Add(flashcard.FlashcardCollection.Title);
            return new SearchDocument
            {
                Id = $"flashcard-{flashcard.FlashcardId}", 
                DataType = "Flashcard",
                Title = title?.Length > 150 ? title.Substring(0, 150) + "..." : title,
                Content = string.Join(
                    " | ",
                    contentParts.Where(s => !string.IsNullOrWhiteSpace(s))
                ),
                LastUpdated = DateTime.UtcNow,

                // Flashcard specific
                FlashcardId = flashcard.FlashcardId,
                CollectionId = flashcard.CollectionId,
                CollectionTitle = flashcard.FlashcardCollection?.Title,
                FrontText = flashcard.FrontText,
                BackText = flashcard.BackText,
                FrontImagePath = flashcard.FrontImagePath,
                BackImagePath = flashcard.BackImagePath,
                Suggest = suggestInputs.Any() ? new CompletionField { Input = suggestInputs } : null
            };
        }
        private SearchDocument MapPartyToSearchDocument(Party party){
            var contentParts = new List<string?> {party.history, party.politics, party.partyProgram};
            string? title = party.partyName;
            var suggestInputs = new List<string>();
            if (!string.IsNullOrWhiteSpace(party.partyName)) suggestInputs.Add(party.partyName);
            if (!string.IsNullOrWhiteSpace(party.partyShortName)) suggestInputs.Add(party.partyShortName);

            return new SearchDocument{
                Id = $"party-{party.partyId}",
                DataType = "Party",
                Title = title?.Length > 150 ? title.Substring(0, 150) + "..." : title,
                Content = string.Join(" | ",
                                        contentParts.Where(s => !string.IsNullOrWhiteSpace(s))),
                LastUpdated = DateTime.UtcNow,

                //Party Specific
                partyName = party.partyName,
                partyShortNameFromParty = party.partyShortName,
                partyProgram = party.partyProgram,
                politics = party.politics,
                history = party.history,
                Suggest = suggestInputs.Any() ? new CompletionField { Input = suggestInputs } : null
            };
        }
        private SearchDocument MapPageToSearchDocument(Page page){

            var contentParts = new List<string?> {page.Title, page.Content};
            string? title = page.Title;
            var suggestInputs = new List<string>();

            if (!string.IsNullOrWhiteSpace(page.Title)) suggestInputs.Add(page.Title);

            return new SearchDocument{
                Id = $"page-{page.Id}",
                DataType = "Page",
                Title = title?.Length > 150 ? title.Substring(0, 150) + "..." : title,
                Content = string.Join(" | ", contentParts.Where(s => !string.IsNullOrWhiteSpace(s))),
                LastUpdated = DateTime.UtcNow,

                pageTitle = page.Title,
                pageContent = page.Content,
                Suggest = suggestInputs.Any() ? new CompletionField { Input = suggestInputs } : null
            };
        }

        private async Task SendBulkRequestAsync(
            List<IBulkOperation> operations,
            CancellationToken cancellationToken
        )
        {
            if (!operations.Any())
                return;

            var bulkRequest = new BulkRequest(IndexName) { Operations = operations };
            var response = await _openSearchClient.BulkAsync(bulkRequest, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError(
                    "Bulk indexing request failed: {ErrorReason}. DebugInfo: {DebugInfo}",
                    response.ServerError?.Error?.Reason ?? "N/A",
                    response.DebugInformation
                );
                throw response.OriginalException
                    ?? new Exception("Bulk request failed with no specific exception.");
            }
            else if (response.Errors)
            {
                _logger.LogWarning("Bulk indexing completed with some item errors.");
                foreach (var itemWithError in response.ItemsWithErrors)
                {
                    _logger.LogWarning(
                        "Failed to index item {ItemId} (Type: {ItemType}): {ErrorReason}",
                        itemWithError.Id,
                        itemWithError.Index,
                        itemWithError.Error?.Reason ?? "Unknown reason"
                    );
                }
            }
            else
            {
                _logger.LogDebug(
                    "Successfully indexed bulk request with {Count} operations.",
                    operations.Count
                );
            }
        }
    }

    public static class SearchIndexSetup
    {
        public static async Task EnsureIndexExistsWithMapping(IServiceProvider services)
        {
            var client = services.GetRequiredService<IOpenSearchClient>();
            var logger = services.GetRequiredService<ILogger<Program>>(); 
            const string indexName = "borgertinget-search"; 

            logger.LogInformation("Checking if index '{IndexName}' exists...", indexName);

            // 1. Check if the index already exists
            var indexExistsResponse = await client.Indices.ExistsAsync(indexName);

            if (!indexExistsResponse.IsValid)
            {
                logger.LogError(
                    "Failed to check index existence for '{IndexName}'. Error: {Error}",
                    indexName,
                    indexExistsResponse.ServerError?.Error?.Reason
                        ?? indexExistsResponse.DebugInformation
                );
                return;
            }

            if (indexExistsResponse.Exists)
            {
                logger.LogInformation("Index '{IndexName}' already exists.", indexName);
                var getMappingResponse = await client.Indices.GetMappingAsync<SearchDocument>(m => m.Index(indexName));
                bool suggestFieldExists = false;
                if (getMappingResponse.IsValid && getMappingResponse.Indices.TryGetValue(indexName, out var indexMapping))
                {
                    if (indexMapping.Mappings.Properties.TryGetValue("suggest", out var suggestProperty) && 
                        suggestProperty is CompletionProperty)
                    {
                        suggestFieldExists = true;
                        logger.LogInformation("'Suggest' field with completion mapping already exists in '{IndexName}'.", indexName);
                    }
                }

                if (!suggestFieldExists)
                {
                    logger.LogInformation("'Suggest' field mapping not found or incorrect in '{IndexName}'. Attempting to update mapping...", indexName);
                    var updateMappingResponse = await client.Indices.PutMappingAsync<SearchDocument>(pm => pm
                        .Index(indexName)
                        .Properties(ps => ps
                            .Completion(c => c
                                .Name(p => p.Suggest) // Map to the Suggest property in SearchDocument
                            )
                        )
                    );

                    if (updateMappingResponse.IsValid && updateMappingResponse.Acknowledged)
                    {
                        logger.LogInformation("Successfully updated mapping for '{IndexName}' to include 'Suggest' field.", indexName);
                    }
                    else
                    {
                        logger.LogError("Failed to update mapping for '{IndexName}'. Error: {Error}. DebugInfo: {DebugInfo}",
                            indexName, 
                            updateMappingResponse.ServerError?.Error?.Reason ?? "N/A", 
                            updateMappingResponse.DebugInformation);
                    }
                }
                return;
            }

            // Create the index with the explicit mapping
            logger.LogInformation(
                "Index '{IndexName}' does not exist. Creating index with mapping...",
                indexName
            );

            var createIndexResponse = await client.Indices.CreateAsync(
                indexName,
                c =>
                    c.Map<SearchDocument>(m =>
                        m 
                        .Properties(ps =>
                            ps
                            // --- Common Fields ---
                            .Keyword(k => k.Name(p => p.Id)) // Keyword for exact matching ID
                                .Keyword(k => k.Name(p => p.DataType)) // Keyword for filtering type
                                .Text(t =>
                                    t.Name(p => p.Title) // Text for full-text search on title
                                        .Fields(f =>
                                            f 
                                            .Keyword(k => k.Name("keyword").IgnoreAbove(256)) // Common pattern
                                        )
                                )
                                .Text(t =>
                                    t.Name(p => p.Content)
                                    .Analyzer("danish")
                                )
                                .Date(d => d.Name(p => p.LastUpdated))
                                // --- Aktor Specific Fields ---
                                .Text(t =>
                                    t.Name(p => p.AktorName)
                                        .Fields(f =>
                                            f.Keyword(k => k.Name("keyword").IgnoreAbove(256))
                                        )
                                )
                                .Text(t =>
                                    t.Name(p => p.Party)
                                        .Fields(f =>
                                            f.Keyword(k => k.Name("keyword").IgnoreAbove(256))
                                        )
                                )
                                .Keyword(k => k.Name(p => p.PartyShortname))
                                .Keyword(k => k.Name(p => p.PictureUrl).Index(false)) 
                                .Text(t =>
                                    t.Name(p => p.MinisterTitle)
                                        .Fields(f =>
                                            f.Keyword(k => k.Name("keyword").IgnoreAbove(256))
                                        )
                                )
                                .Text(t =>
                                    t.Name(p => p.Constituencies)
                                        .Fields(f => f.Keyword(k => k.Name("keyword")))
                                )
                                // --- Flashcard Specific Fields ---
                                .Number(n => n.Name(p => p.FlashcardId).Type(NumberType.Integer)) // Explicit number type
                                .Number(n => n.Name(p => p.CollectionId).Type(NumberType.Integer))
                                .Text(t =>
                                    t.Name(p => p.CollectionTitle)
                                        .Fields(f =>
                                            f.Keyword(k => k.Name("keyword").IgnoreAbove(256))
                                        )
                                )
                                .Text(t => t.Name(p => p.FrontText)) // Analyze front text
                                .Text(t => t.Name(p => p.BackText)) // Analyze back text
                                .Keyword(k => k.Name(p => p.FrontImagePath).Index(false)) // Don't index image paths for search
                                .Keyword(k => k.Name(p => p.BackImagePath).Index(false))
                        
                                .Completion(cp => cp.Name(p => p.Suggest))
                        )
                    )
            );

            if (!createIndexResponse.IsValid)
            {
                logger.LogError(
                    "Failed to create index '{IndexName}'. Error: {Error}",
                    indexName,
                    createIndexResponse.ServerError?.Error?.Reason
                        ?? createIndexResponse.DebugInformation
                );
                // Handle error
            }
            else
            {
                logger.LogInformation(
                    "Successfully created index '{IndexName}' with explicit mapping.",
                    indexName
                );
            }
        }
    }
}
