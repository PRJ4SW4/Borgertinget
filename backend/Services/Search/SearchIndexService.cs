// backend/Services/Search/SearchIndexService.cs
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

namespace backend.Services.Search
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

            // ... (rest of RunFullIndexAsync method remains the same - no changes needed here for mapping)
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
                var parties = await _dbContext.Party.AsNoTracking().ToListAsync(cancellationToken);

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
                _logger.LogInformation("Finished mapping Parties.");

                //Index Learning env
                _logger.LogInformation("Fetching pages");
                var pages = await _dbContext.Pages.AsNoTracking().ToListAsync(cancellationToken);

                foreach(var page in pages){
                    var searchDoc = MapPageToSearchDocument(page);
                    operations.Add(
                        new BulkIndexOperation<SearchDocument>(searchDoc) {Id = searchDoc.Id}
                    );
                    pagesCount++;
                    if (operations.Count >= batchSize){
                        await SendBulkRequestAsync(operations, cancellationToken);
                        operations.Clear();
                         _logger.LogInformation(
                            "Indexed batch of {BatchSize} Page documents...",
                            batchSize
                        );
                    }
                }
                _logger.LogInformation("Finished mapping pages.");

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
                    "Finished indexing. Total Aktors: {AktorCount}, Total Flashcards: {FlashcardCount}, Total Parties: {PartyCount}, Total Pages: {PagesCount}",
                    aktorCount,
                    flashcardCount,
                    partyCount,
                    pagesCount
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

        private SearchDocument MapAktorToSearchDocument(Aktor aktor)
        {
            var contentParts = new List<string?>
            {
                aktor.navn, aktor.Party, aktor.PartyShortname, aktor.MinisterTitel,
                aktor.FunctionFormattedTitle, aktor.PositionsOfTrust, aktor.Sex, aktor.Born,
            };
            if (aktor.Spokesmen != null) contentParts.AddRange(aktor.Spokesmen);
            if (aktor.Ministers != null) contentParts.AddRange(aktor.Ministers);
            if (aktor.ParliamentaryPositionsOfTrust != null) contentParts.AddRange(aktor.ParliamentaryPositionsOfTrust);
            if (aktor.Constituencies != null) contentParts.AddRange(aktor.Constituencies);
            if (aktor.Educations != null) contentParts.AddRange(aktor.Educations);
            if (aktor.Occupations != null) contentParts.AddRange(aktor.Occupations);

            var suggestInputs = new List<string>();
            if (!string.IsNullOrWhiteSpace(aktor.navn)) suggestInputs.Add(aktor.navn);
            if (!string.IsNullOrWhiteSpace(aktor.Party)) suggestInputs.Add(aktor.Party);
            if (!string.IsNullOrWhiteSpace(aktor.PartyShortname)) suggestInputs.Add(aktor.PartyShortname);

            return new SearchDocument
            {
                Id = $"aktor-{aktor.Id}", DataType = "Aktor", Title = aktor.navn,
                Content = string.Join(" | ", contentParts.Where(s => !string.IsNullOrWhiteSpace(s))),
                LastUpdated = DateTime.UtcNow, AktorName = aktor.navn, Party = aktor.Party,
                PartyShortname = aktor.PartyShortname, MinisterTitle = aktor.MinisterTitel,
                Constituencies = aktor.Constituencies?.ToList(), // Ensure it's a List if not null
                Suggest = suggestInputs.Any() ? new CompletionField { Input = suggestInputs } : null
            };
        }

        private SearchDocument MapFlashcardToSearchDocument(Flashcard flashcard)
        {
            var contentParts = new List<string?> { flashcard.FrontText, flashcard.BackText };
            string? title = flashcard.FrontContentType == FlashcardContentType.Text
                ? flashcard.FrontText
                : $"Flashcard from '{flashcard.FlashcardCollection?.Title ?? "Unknown Collection"}'";

            var suggestInputs = new List<string?>();
            if (!string.IsNullOrWhiteSpace(title)) suggestInputs.Add(title);
            if (!string.IsNullOrWhiteSpace(flashcard.FrontText)) suggestInputs.Add(flashcard.FrontText);
            if (!string.IsNullOrWhiteSpace(flashcard.BackText)) suggestInputs.Add(flashcard.BackText);
            if (flashcard.FlashcardCollection != null && !string.IsNullOrWhiteSpace(flashcard.FlashcardCollection.Title))
            {
                suggestInputs.Add(flashcard.FlashcardCollection.Title);
            }

            return new SearchDocument
            {
                Id = $"flashcard-{flashcard.FlashcardId}", DataType = "Flashcard",
                Title = title?.Length > 150 ? title.Substring(0, 150) + "..." : title,
                Content = string.Join(" | ", contentParts.Where(s => !string.IsNullOrWhiteSpace(s))),
                LastUpdated = DateTime.UtcNow, FlashcardId = flashcard.FlashcardId,
                CollectionId = flashcard.CollectionId, CollectionTitle = flashcard.FlashcardCollection?.Title,
                FrontText = flashcard.FrontText, BackText = flashcard.BackText,
                FrontImagePath = flashcard.FrontImagePath, BackImagePath = flashcard.BackImagePath,
                Suggest = suggestInputs.Any(s => !string.IsNullOrWhiteSpace(s)) ? new CompletionField { Input = suggestInputs.Where(s => !string.IsNullOrWhiteSpace(s)).ToList()! } : null
            };
        }

        private SearchDocument MapPartyToSearchDocument(Party party)
        {
            var contentParts = new List<string?> { party.history, party.politics, party.partyProgram };
            string? title = party.partyName;
            var suggestInputs = new List<string>();
            if (!string.IsNullOrWhiteSpace(party.partyName)) suggestInputs.Add(party.partyName);
            if (!string.IsNullOrWhiteSpace(party.partyShortName)) suggestInputs.Add(party.partyShortName);

            return new SearchDocument
            {
                Id = $"party-{party.partyId}", DataType = "Party",
                Title = title?.Length > 150 ? title.Substring(0, 150) + "..." : title,
                Content = string.Join(" | ", contentParts.Where(s => !string.IsNullOrWhiteSpace(s))),
                LastUpdated = DateTime.UtcNow, partyName = party.partyName,
                partyShortNameFromParty = party.partyShortName, partyProgram = party.partyProgram,
                politics = party.politics, history = party.history,
                Suggest = suggestInputs.Any() ? new CompletionField { Input = suggestInputs } : null
            };
        }

        private SearchDocument MapPageToSearchDocument(Page page)
        {
            var contentParts = new List<string?> { page.Title, page.Content };
            string? title = page.Title;
            var suggestInputs = new List<string>();
            if (!string.IsNullOrWhiteSpace(page.Title)) suggestInputs.Add(page.Title);

            return new SearchDocument
            {
                Id = $"page-{page.Id}", DataType = "Page",
                Title = title?.Length > 150 ? title.Substring(0, 150) + "..." : title,
                Content = string.Join(" | ", contentParts.Where(s => !string.IsNullOrWhiteSpace(s))),
                LastUpdated = DateTime.UtcNow, pageTitle = page.Title, pageContent = page.Content,
                Suggest = suggestInputs.Any() ? new CompletionField { Input = suggestInputs } : null
            };
        }

        private async Task SendBulkRequestAsync(List<IBulkOperation> operations, CancellationToken cancellationToken)
        {
            if (!operations.Any()) return;
            var bulkRequest = new BulkRequest(IndexName) { Operations = operations };
            var response = await _openSearchClient.BulkAsync(bulkRequest, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Bulk indexing request failed: {ErrorReason}. DebugInfo: {DebugInfo}",
                    response.ServerError?.Error?.Reason ?? "N/A", response.DebugInformation);
                throw response.OriginalException ?? new Exception("Bulk request failed with no specific exception.");
            }
            else if (response.Errors)
            {
                _logger.LogWarning("Bulk indexing completed with some item errors.");
                foreach (var itemWithError in response.ItemsWithErrors)
                {
                    _logger.LogWarning("Failed to index item {ItemId} (Type: {ItemType}): {ErrorReason}",
                        itemWithError.Id, itemWithError.Index, itemWithError.Error?.Reason ?? "Unknown reason");
                }
            }
            else
            {
                _logger.LogDebug("Successfully indexed bulk request with {Count} operations.", operations.Count);
            }
        }
    }
    public static class SearchIndexSetup
    {
        public static async Task EnsureIndexExistsWithMapping(IServiceProvider services)
        {
            var client = services.GetRequiredService<IOpenSearchClient>();
            var logger = services.GetRequiredService<ILogger<SearchIndexingService>>(); // Changed logger category
            const string indexName = "borgertinget-search";

            logger.LogInformation("[SearchIndexSetup] Starting EnsureIndexExistsWithMapping for index '{IndexName}'.", indexName);

            var indexExistsResponse = await client.Indices.ExistsAsync(indexName);

            // --- MODIFIED LOGIC TO HANDLE 404 CORRECTLY ---
            bool indexTrulyExists = false;
            if (indexExistsResponse.IsValid) // If the response is considered valid by the client
            {
                indexTrulyExists = indexExistsResponse.Exists;
            }
            else if (indexExistsResponse.ApiCall is { Success: true, HttpStatusCode: 404 }) // Specifically handle the 404 case
            {
                logger.LogInformation("[SearchIndexSetup] Index '{IndexName}' does not exist (confirmed by 404).", indexName);
                indexTrulyExists = false;
            }
            else // Any other invalid response is an actual error
            {
                logger.LogError("[SearchIndexSetup] Failed to check index existence for '{IndexName}'. Status: {StatusCode}. Error: {Error}. DebugInfo: {DebugInfo}",
                    indexName,
                    indexExistsResponse.ApiCall?.HttpStatusCode,
                    indexExistsResponse.ServerError?.Error?.Reason ?? "N/A",
                    indexExistsResponse.DebugInformation);
                return; // Exit if we can't reliably determine existence
            }
            // --- END OF MODIFIED LOGIC ---

            if (indexTrulyExists)
            {
                logger.LogInformation("[SearchIndexSetup] Index '{IndexName}' already exists. Verifying 'suggest' field mapping...", indexName);
                // You might want to add logic here to GET the current mapping and verify it.
                // For now, we'll assume if it exists, we won't try to recreate or update mapping.
                // If issues persist, you might need to delete and recreate the index if the mapping is wrong.
                logger.LogInformation("[SearchIndexSetup] Index '{IndexName}' exists. If 'suggest' field is not working as expected, consider deleting the index manually and restarting the application to ensure the latest mapping is applied.", indexName);
                return;
            }

            // If we reach here, indexTrulyExists is false, so we create the index.
            logger.LogInformation("[SearchIndexSetup] Index '{IndexName}' does not exist. Attempting to create index with fully explicit mapping...", indexName);

            var createIndexResponse = await client.Indices.CreateAsync(indexName, c => c
                .Map(m => m
                    .Properties<SearchDocument>(ps => ps
                        // --- Common Fields ---
                        .Keyword(k => k.Name(p => p.Id))
                        .Keyword(k => k.Name(p => p.DataType))
                        .Text(t => t.Name(p => p.Title).Fields(f => f.Keyword(k => k.Name("keyword").IgnoreAbove(256))))
                        .Text(t => t.Name(p => p.Content))
                        .Date(d => d.Name(p => p.LastUpdated))
                        // --- Aktor Specific Fields ---
                        .Text(t => t.Name(p => p.AktorName).Fields(f => f.Keyword(k => k.Name("keyword").IgnoreAbove(256))))
                        .Text(t => t.Name(p => p.Party).Fields(f => f.Keyword(k => k.Name("keyword").IgnoreAbove(256))))
                        .Keyword(k => k.Name(p => p.PartyShortname))
                        .Keyword(k => k.Name(p => p.PictureUrl).Index(false))
                        .Text(t => t.Name(p => p.MinisterTitle).Fields(f => f.Keyword(k => k.Name("keyword").IgnoreAbove(256))))
                        .Text(t => t.Name(p => p.Constituencies).Fields(f => f.Keyword(k => k.Name("keyword"))))
                        // --- Flashcard Specific Fields ---
                        .Number(n => n.Name(p => p.FlashcardId).Type(NumberType.Integer))
                        .Number(n => n.Name(p => p.CollectionId).Type(NumberType.Integer))
                        .Text(t => t.Name(p => p.CollectionTitle).Fields(f => f.Keyword(k => k.Name("keyword").IgnoreAbove(256))))
                        .Text(t => t.Name(p => p.FrontText))
                        .Text(t => t.Name(p => p.BackText))
                        .Keyword(k => k.Name(p => p.FrontImagePath).Index(false))
                        .Keyword(k => k.Name(p => p.BackImagePath).Index(false))
                        // --- Party Specific Fields (from SearchDocument) ---
                        .Text(t => t.Name(p => p.partyName).Fields(f => f.Keyword(k => k.Name("keyword").IgnoreAbove(256))))
                        .Keyword(k => k.Name(p => p.partyShortNameFromParty))
                        .Text(t => t.Name(p => p.partyProgram))
                        .Text(t => t.Name(p => p.politics))
                        .Text(t => t.Name(p => p.history))
                        // --- Page Specific Fields (from SearchDocument) ---
                        .Text(t => t.Name(p => p.pageTitle).Fields(f => f.Keyword(k => k.Name("keyword").IgnoreAbove(256))))
                        .Text(t => t.Name(p => p.pageContent))
                        // --- CRUCIAL: Explicitly map the 'Suggest' field as Completion ---
                        .Completion(cp => cp
                            .Name(p => p.Suggest) // This should map SearchDocument.Suggest
                        )
                    )
                )
            );

            if (!createIndexResponse.IsValid)
            {
                logger.LogError("[SearchIndexSetup] Failed to create index '{IndexName}'. Error: {Error}. DebugInfo: {DebugInfo}",
                    indexName, createIndexResponse.ServerError?.Error?.Reason ?? "N/A", createIndexResponse.DebugInformation);
                // Log the full exception if available
                if (createIndexResponse.OriginalException != null)
                {
                    logger.LogError(createIndexResponse.OriginalException, "[SearchIndexSetup] Exception during index creation for '{IndexName}'.", indexName);
                }
            }
            else
            {
                logger.LogInformation("[SearchIndexSetup] Successfully created index '{IndexName}'.", indexName);
            }
            logger.LogInformation("[SearchIndexSetup] Finished EnsureIndexExistsWithMapping for index '{IndexName}'.", indexName);
        }
    }
}