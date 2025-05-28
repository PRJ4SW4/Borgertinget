using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models; //adgang til alle modeller
using backend.Services.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IOpenSearchClient _openSearchClient;
        private readonly ILogger<SearchController> _logger;
        private readonly SearchIndexingService _searchIndexingService;
        private readonly IServiceProvider _serviceProvider;
        private const string IndexName = "borgertinget-search";
        private const int TopNResults = 5;
        private const string SuggestionName = "search-suggester";

        public SearchController(
            IOpenSearchClient openSearchClient,
            ILogger<SearchController> logger,
            SearchIndexingService searchIndexingService,
            IServiceProvider serviceProvider
        )
        {
            _openSearchClient = openSearchClient;
            _logger = logger;
            _searchIndexingService = searchIndexingService;
            _serviceProvider = serviceProvider;
        }

        //Multimatch query
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SearchDocument>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Authorize]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty.");
            }

            _logger.LogInformation(
                "Received search query: for top {TopNResults} results",
                TopNResults
            );

            try
            { //opensearch query
                var searchResponse = await _openSearchClient.SearchAsync<SearchDocument>(s =>
                    s.Index(IndexName)
                        .Size(TopNResults)
                        .Query(q =>
                            q.MultiMatch(mm =>
                                mm.Query(query)
                                    .Fields(f =>
                                        f.Field(sd => sd.Title, boost: 3)
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
                                    .Type(TextQueryType.BestFields)
                                    .Fuzziness(Fuzziness.Auto) // Allows for some typos
                                    .PrefixLength(1) // How many characters must match at the beginning
                            )
                        )
                );

                if (!searchResponse.IsValid) //not valid
                {
                    _logger.LogError(
                        "OpenSearch query failed: {ErrorReason}. DebugInfo: {DebugInfo}",
                        searchResponse.ServerError?.Error?.Reason ?? "N/A",
                        searchResponse.DebugInformation
                    );
                    return StatusCode(500, "An error occurred while searching.");
                }

                _logger.LogInformation(
                    "Search successful. Returning {Count} documents out of {TotalHits} total hits",
                    searchResponse.Documents.Count,
                    searchResponse.Total
                );
                return Ok(searchResponse.Documents); //return response from opensearch
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred during search operation:");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        //returns the suggestion field from query
        [HttpGet("suggest")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Authorize]
        public async Task<IActionResult> Suggest([FromQuery] string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return Ok(Enumerable.Empty<string>());
            }

            _logger.LogInformation("Received suggestion request");

            try
            {
                var suggestResponse = await _openSearchClient.SearchAsync<SearchDocument>(s =>
                    s.Index(IndexName)
                        .Suggest(su =>
                            su.Completion(
                                SuggestionName,
                                cs =>
                                    cs.Field(f => f.Suggest)
                                        .Prefix(prefix)
                                        .Fuzzy(f => f.Fuzziness(Fuzziness.Auto)) //fuzzy, allows misspelling
                                        .Size(5) // Number of suggestions to return
                            )
                        )
                        .Source(false)
                );

                if (!suggestResponse.IsValid) //error in query
                {
                    _logger.LogError(
                        "OpenSearch suggest query failed: {ErrorReason}. DebugInfo: {DebugInfo}",
                        suggestResponse.ServerError?.Error?.Reason ?? "N/A",
                        suggestResponse.DebugInformation
                    );
                    return StatusCode(500, "An error occurred while fetching suggestions.");
                }

                var suggestions = new List<string>(); //list til suggestions
                var completionSuggest = suggestResponse.Suggest[SuggestionName];
                if (completionSuggest != null)
                {
                    foreach (var option in completionSuggest.SelectMany(s => s.Options))
                    {
                        suggestions.Add(option.Text);
                    }
                }

                var distinctSuggestions = suggestions.Distinct().ToList(); //unikke suggestions

                _logger.LogInformation(
                    "Suggestion query successful. Returning {Count} distinct suggestions:'",
                    distinctSuggestions.Count
                );
                return Ok(distinctSuggestions); //return suggestions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred during suggest operation:");
                return StatusCode(
                    500,
                    "An internal server error occurred while fetching suggestions."
                );
            }
        }

        //Til indexering, indeholder logik til at oprette index struktur, hvis index ikke ekstistere, ADMIN
        [HttpPost("ensure-and-reindex")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EnsureAndReindex()
        {
            _logger.LogInformation("Ensure and Reindex endpoint called.");
            try
            {
                // Ensure the index is created, if not run EnsureIndexExistsWithMapping
                _logger.LogInformation(
                    "Ensuring OpenSearch index '{IndexName}' exists with mapping.",
                    IndexName
                );
                await SearchIndexSetup.EnsureIndexExistsWithMapping(_serviceProvider); //
                _logger.LogInformation(
                    "Index check/creation with mapping complete for '{IndexName}'.",
                    IndexName
                );

                //Run a full indexing
                _logger.LogInformation(
                    "Triggering full background indexing task for '{IndexName}'.",
                    IndexName
                );
                await _searchIndexingService.RunFullIndexAsync(); //
                _logger.LogInformation(
                    "Full indexing task triggered successfully for '{IndexName}'.",
                    IndexName
                );

                return Ok(
                    new
                    {
                        message = $"Index '{IndexName}' ensured and full re-indexing process initiated.",
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    ex,
                    "A critical error occurred during the ensure and reindex process for '{IndexName}'.",
                    IndexName
                );
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "An error occurred during the ensure and reindex process.",
                        error = ex.Message,
                    }
                );
            }
        }
    }
}
