using Microsoft.AspNetCore.Mvc;
using OpenSearch.Client;
using backend.Models; // Assuming your SearchDocument is here
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IOpenSearchClient _openSearchClient;
        private readonly ILogger<SearchController> _logger;
        private const string IndexName = "borgertinget-search"; // From your SearchIndexingService
        private const int TopNResults = 5; // Define the number of results to return

        public SearchController(IOpenSearchClient openSearchClient, ILogger<SearchController> logger)
        {
            _openSearchClient = openSearchClient;
            _logger = logger;
        }

        /// <summary>
        /// Searches documents in the OpenSearch index based on a query string.
        /// It performs a multi-match query across several fields of the SearchDocument
        /// and returns the top 5 matching results.
        /// </summary>
        /// <param name="query">The search term.</param>
        /// <returns>A list of the top 5 search documents matching the query.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SearchDocument>), 200)]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty.");
            }

            _logger.LogInformation("Received search query: '{Query}' for top {TopNResults} results", query, TopNResults);

            try
            {
                var searchResponse = await _openSearchClient.SearchAsync<SearchDocument>(s => s
                    .Index(IndexName)
                    .Size(TopNResults) // Set size to return only the top N results
                    .Query(q => q
                        .MultiMatch(mm => mm
                            .Query(query)
                            .Fields(f => f
                                .Field(sd => sd.Title, boost: 3)
                                .Field(sd => sd.DataType)
                                .Field(sd => sd.Content)
                                .Field(sd => sd.AktorName, boost: 2)
                                .Field(sd => sd.Party)
                                .Field(sd => sd.PartyShortname)
                                .Field(sd => sd.MinisterTitle)
                                .Field(sd => sd.CollectionTitle)
                                .Field(sd => sd.FrontText)
                                .Field(sd => sd.BackText)
                                .Field(sd => sd.Constituencies)
                            )
                            .Type(TextQueryType.BestFields)
                            .Fuzziness(Fuzziness.Auto)
                            .PrefixLength(1)
                        )
                    )
                );

                if (!searchResponse.IsValid)
                {
                    _logger.LogError("OpenSearch query failed: {ErrorReason}. DebugInfo: {DebugInfo}",
                        searchResponse.ServerError?.Error?.Reason ?? "N/A",
                        searchResponse.DebugInformation);
                    return StatusCode(500, "An error occurred while searching.");
                }

                _logger.LogInformation("Search successful. Returning {Count} documents out of {TotalHits} total hits for query: '{Query}'", searchResponse.Documents.Count, searchResponse.Total, query);
                
                // Just return the documents, as total hits and pagination info are less relevant for a top N query
                return Ok(searchResponse.Documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred during search operation for query: '{Query}'", query);
                return StatusCode(500, "An internal server error occurred.");
            }
        }
    }
}