// Example: Services/SearchIndexingService.cs (New or Modified File)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend.Data; // Your DbContext
using backend.Models; // Your models including Aktor, Flashcard, SearchDocument
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql; // Or your DB provider if not using Npgsql directly here
using OpenSearch.Client;
using OpenSearch.Net; // For BulkResponse

using Microsoft.Extensions.DependencyInjection; // For GetService
using Microsoft.Extensions.Hosting; // For IHostApplicationLifetime or similar context
using Microsoft.Extensions.Logging; // For logging

// --- Assume you have configured and registered IOpenSearchClient via DI ---



namespace backend.Services
{
    public class SearchIndexingService // Renamed for clarity
    {
        private readonly IOpenSearchClient _openSearchClient;
        private readonly DataContext _dbContext; // Inject DbContext directly
        private readonly ILogger<SearchIndexingService> _logger;
        private const string IndexName = "borgertinget-search"; // Define your OpenSearch index name

        public SearchIndexingService(IOpenSearchClient openSearchClient, DataContext dbContext, ILogger<SearchIndexingService> logger)
        {
            _openSearchClient = openSearchClient ?? throw new ArgumentNullException(nameof(openSearchClient));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task RunFullIndexAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting full indexing from PostgreSQL to OpenSearch index '{IndexName}'...", IndexName);

            var bulkRequest = new BulkRequest(IndexName);
            var operations = new List<IBulkOperation>();
            int aktorCount = 0;
            int flashcardCount = 0;
            const int batchSize = 1000; // Adjust batch size as needed

            try
            {
                // --- Index Aktors ---
                _logger.LogInformation("Fetching Aktors from database...");
                // Fetch in batches if the dataset is very large, but for now fetch all
                var aktors = await _dbContext.Aktor
                                     .AsNoTracking() // Improve performance for read-only operation
                                     .Where(a => a.typeid == 5) // Assuming you only index politicians
                                     .ToListAsync(cancellationToken);

                _logger.LogInformation("Mapping {Count} Aktors for indexing...", aktors.Count);
                foreach (var aktor in aktors)
                {
                    var searchDoc = MapAktorToSearchDocument(aktor);
                    operations.Add(new BulkIndexOperation<SearchDocument>(searchDoc) { Id = searchDoc.Id });
                    aktorCount++;

                    if (operations.Count >= batchSize)
                    {
                        await SendBulkRequestAsync(operations, cancellationToken);
                        operations.Clear();
                        _logger.LogInformation("Indexed batch of {BatchSize} documents...", batchSize);
                    }
                }
                _logger.LogInformation("Finished mapping Aktors.");

                // --- Index Flashcards ---
                _logger.LogInformation("Fetching Flashcards from database...");
                var flashcards = await _dbContext.Flashcards
                                         .AsNoTracking()
                                         .Include(f => f.FlashcardCollection) // Include collection for title
                                         .ToListAsync(cancellationToken);

                _logger.LogInformation("Mapping {Count} Flashcards for indexing...", flashcards.Count);
                foreach (var flashcard in flashcards)
                {
                    var searchDoc = MapFlashcardToSearchDocument(flashcard);
                    operations.Add(new BulkIndexOperation<SearchDocument>(searchDoc) { Id = searchDoc.Id });
                    flashcardCount++;

                    if (operations.Count >= batchSize)
                    {
                        await SendBulkRequestAsync(operations, cancellationToken);
                        operations.Clear();
                         _logger.LogInformation("Indexed batch of {BatchSize} documents...", batchSize);
                    }
                }
                 _logger.LogInformation("Finished mapping Flashcards.");


                // Send any remaining documents
                if (operations.Count > 0)
                {
                    await SendBulkRequestAsync(operations, cancellationToken);
                     _logger.LogInformation("Indexed final batch of {Count} documents.", operations.Count);
                }

                _logger.LogInformation("Finished indexing. Total Aktors: {AktorCount}, Total Flashcards: {FlashcardCount}", aktorCount, flashcardCount);

            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                 _logger.LogWarning("Indexing operation was cancelled.");
                 // Handle cancellation gracefully if needed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during indexing process.");
                // Rethrow or handle exceptions appropriately
                throw;
            }
        }

        // Helper to map Aktor to the unified SearchDocument
        private SearchDocument MapAktorToSearchDocument(Aktor aktor)
        {
            // Combine relevant text fields for searchable content
            var contentParts = new List<string?>
            {
                aktor.Party,
                aktor.PartyShortname,
                aktor.MinisterTitel,
                aktor.FunctionFormattedTitle,
                aktor.PositionsOfTrust
                // Add other text fields you want searchable here
            };
            contentParts.AddRange(aktor.Spokesmen ?? Enumerable.Empty<string>());
             contentParts.AddRange(aktor.Ministers ?? Enumerable.Empty<string>());
             contentParts.AddRange(aktor.ParliamentaryPositionsOfTrust ?? Enumerable.Empty<string>());


            return new SearchDocument
            {
                Id = $"aktor-{aktor.Id}", // Unique ID for OpenSearch
                DataType = "Aktor",
                Title = aktor.navn, // Use full name as the primary title
                Content = string.Join(" | ", contentParts.Where(s => !string.IsNullOrWhiteSpace(s))), // Simple concatenation
                LastUpdated = DateTime.UtcNow, // Use UTC

                // Aktor specific
                AktorName = aktor.navn,
                Party = aktor.Party,
                PartyShortname = aktor.PartyShortname,
                PictureUrl = aktor.PictureMiRes,
                MinisterTitle = aktor.MinisterTitel,
                Constituencies = aktor.Constituencies
                // Map other Aktor fields as needed
            };
        }

         // Helper to map Flashcard to the unified SearchDocument
        private SearchDocument MapFlashcardToSearchDocument(Flashcard flashcard)
        {
             // Combine front/back text for searchable content
            var contentParts = new List<string?> { flashcard.FrontText, flashcard.BackText };

            string? title = flashcard.FrontContentType == FlashcardContentType.Text
                          ? flashcard.FrontText
                          : $"Flashcard from '{flashcard.FlashcardCollection?.Title ?? "Unknown Collection"}'";


            return new SearchDocument
            {
                Id = $"flashcard-{flashcard.FlashcardId}", // Unique ID
                DataType = "Flashcard",
                Title = title?.Length > 150 ? title.Substring(0, 150) + "..." : title, // Simple title logic, truncate if needed
                Content = string.Join(" | ", contentParts.Where(s => !string.IsNullOrWhiteSpace(s))),
                LastUpdated = DateTime.UtcNow, // Use UTC

                // Flashcard specific
                FlashcardId = flashcard.FlashcardId,
                CollectionId = flashcard.CollectionId,
                CollectionTitle = flashcard.FlashcardCollection?.Title,
                FrontText = flashcard.FrontText,
                BackText = flashcard.BackText,
                FrontImagePath = flashcard.FrontImagePath,
                BackImagePath = flashcard.BackImagePath,
                // No Aktor specific fields populated here
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
                    response.ServerError?.Error?.Reason ?? "N/A",
                    response.DebugInformation);
                // Throw or handle more granularly if needed
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
                // Decide if partial success is acceptable or should throw an error
            }
            else {
                 _logger.LogDebug("Successfully indexed bulk request with {Count} operations.", operations.Count);
            }
        }
    }

    public static class SearchIndexSetup{
        public static async Task EnsureIndexExistsWithMapping(IServiceProvider services)
        {
            // Resolve services needed
            var client = services.GetRequiredService<IOpenSearchClient>();
            var logger = services.GetRequiredService<ILogger<Program>>(); // Or appropriate logger category
            const string indexName = "borgertinget-search"; // Use the same index name as in your indexing service

            logger.LogInformation("Checking if index '{IndexName}' exists...", indexName);

            // 1. Check if the index already exists
            var indexExistsResponse = await client.Indices.ExistsAsync(indexName);

            if (!indexExistsResponse.IsValid)
            {
                logger.LogError("Failed to check index existence for '{IndexName}'. Error: {Error}",
                    indexName, indexExistsResponse.ServerError?.Error?.Reason ?? indexExistsResponse.DebugInformation);
                // Handle error appropriately - maybe throw or exit startup
                return;
            }

            if (indexExistsResponse.Exists)
            {
                logger.LogInformation("Index '{IndexName}' already exists.", indexName);
                // Optional: You could check if the existing mapping needs updates here using GetMapping API,
                // but modifying existing mappings is often limited.
                return;
            }

            // 2. Create the index with the explicit mapping
            logger.LogInformation("Index '{IndexName}' does not exist. Creating index with mapping...", indexName);

            var createIndexResponse = await client.Indices.CreateAsync(indexName, c => c
                .Map<SearchDocument>(m => m // Use the C# model type
                    .Properties(ps => ps
                        // --- Common Fields ---
                        .Keyword(k => k.Name(p => p.Id)) // Keyword for exact matching ID
                        .Keyword(k => k.Name(p => p.DataType)) // Keyword for filtering type
                        .Text(t => t.Name(p => p.Title) // Text for full-text search on title
                            .Fields(f => f // Add keyword sub-field for exact matching/sorting
                                .Keyword(k => k.Name("keyword").IgnoreAbove(256)) // Common pattern
                            )
                        )
                        .Text(t => t.Name(p => p.Content) // Text for main content search
                            // You can customize the analyzer here if needed, e.g., .Analyzer("danish")
                        )
                        .Date(d => d.Name(p => p.LastUpdated)) // Date type

                        // --- Aktor Specific Fields ---
                        .Text(t => t.Name(p => p.AktorName).Fields(f => f.Keyword(k => k.Name("keyword").IgnoreAbove(256))))
                        .Text(t => t.Name(p => p.Party).Fields(f => f.Keyword(k => k.Name("keyword").IgnoreAbove(256))))
                        .Keyword(k => k.Name(p => p.PartyShortname)) // Usually an exact match term
                        .Keyword(k => k.Name(p => p.PictureUrl).Index(false)) // URLs often don't need indexing
                        .Text(t => t.Name(p => p.MinisterTitle).Fields(f => f.Keyword(k => k.Name("keyword").IgnoreAbove(256))))
                        // For lists like Constituencies, map as text/keyword based on search needs
                        .Text(t => t.Name(p => p.Constituencies).Fields(f => f.Keyword(k => k.Name("keyword"))))


                        // --- Flashcard Specific Fields ---
                        .Number(n => n.Name(p => p.FlashcardId).Type(NumberType.Integer)) // Explicit number type
                        .Number(n => n.Name(p => p.CollectionId).Type(NumberType.Integer))
                        .Text(t => t.Name(p => p.CollectionTitle).Fields(f => f.Keyword(k => k.Name("keyword").IgnoreAbove(256))))
                        .Text(t => t.Name(p => p.FrontText)) // Analyze front text
                        .Text(t => t.Name(p => p.BackText))  // Analyze back text
                        .Keyword(k => k.Name(p => p.FrontImagePath).Index(false)) // Don't index image paths for search usually
                        .Keyword(k => k.Name(p => p.BackImagePath).Index(false))

                    // --- Optional: Completion Suggester Field ---
                    // .Completion(cp => cp.Name(p => p.Suggest))
                    )
                // You can also add Index Settings here (e.g., number of shards/replicas)
                // .Settings(s => s.NumberOfShards(1).NumberOfReplicas(0)) // Example for local dev
                )
            );

            if (!createIndexResponse.IsValid)
            {
                logger.LogError("Failed to create index '{IndexName}'. Error: {Error}",
                    indexName, createIndexResponse.ServerError?.Error?.Reason ?? createIndexResponse.DebugInformation);
                // Handle error
            }
            else
            {
                logger.LogInformation("Successfully created index '{IndexName}' with explicit mapping.", indexName);
            }
        }
    }

    
}