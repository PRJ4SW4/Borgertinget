// backend/Controllers/SearchController.cs
using Microsoft.AspNetCore.Mvc;
using OpenSearch.Client;
using backend.Models;
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
        private const string IndexName = "borgertinget-search";
        private const int TopNResults = 5;
        private const string SuggestionName = "search-suggester"; // Name for our suggester

        public SearchController(IOpenSearchClient openSearchClient, ILogger<SearchController> logger)
        {
            _openSearchClient = openSearchClient;
            _logger = logger;
        }

        /// <summary>
        /// Searches documents in the OpenSearch index based on a query string.
        /// Performs a multi-match query across several fields.
        /// </summary>
        /// <param name="query">The search term.</param>
        /// <returns>A list of search documents matching the query.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SearchDocument>), 200)]
        [ProducesResponseType(400)]
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
                    .Size(TopNResults)
                    .Query(q => q
                        .MultiMatch(mm => mm
                            .Query(query)
                            .Fields(f => f
                                .Field(sd => sd.Title, boost: 3)
                                .Field(sd => sd.AktorName, boost: 2)
                                .Field(sd => sd.partyName, boost: 2)
                                .Field(sd => sd.pageTitle, boost: 2)
                                .Field(sd => sd.CollectionTitle, boost: 1.5)
                                .Field(sd => sd.FrontText)
                                .Field(sd => sd.Content) // General content field
                                .Field(sd => sd.Party)
                                .Field(sd => sd.PartyShortname)
                                .Field(sd => sd.MinisterTitle)
                                .Field(sd => sd.BackText)
                                .Field(sd => sd.Constituencies)
                                .Field(sd => sd.partyProgram)
                                .Field(sd => sd.politics)
                                .Field(sd => sd.history)
                                .Field(sd => sd.pageContent)
                                .Field(sd => sd.Title)
                            )
                            .Type(TextQueryType.BestFields) // Or MostFields, CrossFields depending on needs
                            .Fuzziness(Fuzziness.Auto) // Allows for some typos
                            .PrefixLength(1) // How many characters must match at the beginning
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
                return Ok(searchResponse.Documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred during search operation for query: '{Query}'", query);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        /// <summary>
        /// Provides search suggestions based on the input text using a completion suggester.
        /// </summary>
        /// <param name="prefix">The text prefix to get suggestions for.</param>
        /// <returns>A list of suggestion strings.</returns>
        [HttpGet("suggest")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Suggest([FromQuery] string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return Ok(Enumerable.Empty<string>());
            }

            _logger.LogInformation("Received suggestion request for prefix: '{Prefix}'", prefix);

            try
            {
                var suggestResponse = await _openSearchClient.SearchAsync<SearchDocument>(s => s
                    .Index(IndexName)
                    .Suggest(su => su
                        .Completion(SuggestionName, cs => cs 
                            .Field(f => f.Suggest)
                            .Prefix(prefix)
                            .Fuzzy(f => f
                                .Fuzziness(Fuzziness.Auto)
                            )
                            .Size(5) // Number of suggestions to return
                        )
                    )
                    .Source(false)
                );

                if (!suggestResponse.IsValid)
                {
                    _logger.LogError("OpenSearch suggest query failed: {ErrorReason}. DebugInfo: {DebugInfo}",
                        suggestResponse.ServerError?.Error?.Reason ?? "N/A",
                        suggestResponse.DebugInformation);
                    return StatusCode(500, "An error occurred while fetching suggestions.");
                }

                var suggestions = new List<string>();
                var completionSuggest = suggestResponse.Suggest[SuggestionName];
                if (completionSuggest != null)
                {
                    foreach (var option in completionSuggest.SelectMany(s => s.Options))
                    {
                        suggestions.Add(option.Text);
                    }
                }
                
                var distinctSuggestions = suggestions.Distinct().ToList();

                _logger.LogInformation("Suggestion query successful. Returning {Count} distinct suggestions for prefix: '{Prefix}'", distinctSuggestions.Count, prefix);
                return Ok(distinctSuggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred during suggest operation for prefix: '{Prefix}'", prefix);
                return StatusCode(500, "An internal server error occurred while fetching suggestions.");
            }
        }
    }
}