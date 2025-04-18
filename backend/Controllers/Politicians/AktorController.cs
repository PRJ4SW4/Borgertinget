namespace backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using backend.Services;
using backend.Models; // Namespace containing Provider model
using backend.Data; // Namespace for your DbContext (e.g., AppDbContext)
using backend.DTO.FT;
using System.Text.Json;
using Microsoft.Extensions.Configuration;


[ApiController]
[Route("api/[controller]")]
public class AktorController : ControllerBase{
    private readonly DataContext _context;
    private readonly HttpService _httpService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AktorController> _logger; // Add Logger

    public AktorController(DataContext context, HttpService httpService, IConfiguration conf, ILogger<AktorController> logger){
        _context = context;
        _httpService = httpService;
        _configuration = conf;
        _logger = logger;
    }
    //Sender hele listen af politikere med bruger AktorDetailDto
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<Aktor>>> GetAllAktors(){
        var aktors = await _context.Aktor.
                                          Where(a => a.typeid ==5).
                                          OrderBy(a => a.navn).
                                          Select(a => AktorDetailDto.FromAktor(a)).
                                          ToListAsync();

        return Ok(aktors);
    }
    //Sender en politiker, bruger AktorDetailDto
    [HttpGet("{id}")]
    public async Task<ActionResult<Aktor>> GetAktorById(int id)
    {
        try
        {
            // Find the Aktor by primary key (Id)
            // FindAsync is efficient for lookup by primary key
            var aktor = await _context.Aktor.FindAsync(id);

            // Check if an Aktor with the given id was found
            if (aktor == null)
            {
                // Return 404 Not Found if no match
                return NotFound($"No politician found with ID {id}.");
            }
            var aktorDto = AktorDetailDto.FromAktor(aktor);
            // Return the found politician with 200 OK status
            return Ok(aktorDto);
        }
        catch (Exception ex)
        {
            // Log the exception details (consider using a proper logging framework)
            Console.WriteLine($"Error fetching politician with ID {id}: {ex.Message}");
            // Return a generic 500 Internal Server Error
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
    //sender politikere med samme partyName, bruger aktorDetailDto
    [HttpGet("GetParty/{partyName}")] // Route definition: api/Aktor/GetParty/ThePartyName
    public async Task<ActionResult<IEnumerable<Aktor>>> GetParty(string partyName)
    {
        // Basic validation for the input party name
        if (string.IsNullOrWhiteSpace(partyName))
        {
            return BadRequest("Party name cannot be empty."); // Return 400 Bad Request if name is missing
        }

        // Normalize the input party name to lower case for case-insensitive comparison
        var lowerPartyName = partyName.ToLower();

        try
        {
            // Query the database for Aktors
            var filteredPoliticians = await _context.Aktor
                // Filter where either Party or PartyShortname matches (case-insensitive)
                .Where(a => (a.Party != null && a.Party.ToLower() == lowerPartyName) ||
                            (a.PartyShortname != null && a.PartyShortname.ToLower() == lowerPartyName))
                .OrderBy(a => a.navn)
                .Select(a => AktorDetailDto.FromAktor(a))
                .ToListAsync();

            // Return the list of found politicians with 200 OK status
            return Ok(filteredPoliticians);
        }
        catch (Exception ex)
        {
            // Log the exception details (consider using a proper logging framework)
            Console.WriteLine($"Error fetching politicians for party {partyName}: {ex.Message}");
            // Return a generic 500 Internal Server Error
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
    [HttpGet("GetParties")] // Route definition: api/Aktor/GetParties
    public async Task<ActionResult<IEnumerable<string>>> GetParties()
    {
        try
        {
            // Query the Aktor table
            var parties = await _context.Aktor
                // Select the 'Party' field (assuming this is the full name you want to list)
                .Select(a => a.Party)
                // Ensure the party name is not null or just whitespace
                .Where(partyName => !string.IsNullOrWhiteSpace(partyName))
                // Get only unique names
                .Distinct()
                // Order them alphabetically
                .OrderBy(partyName => partyName)
                // Execute the query
                .ToListAsync();

            // Return the distinct list of party names
            return Ok(parties);
        }
        catch (Exception ex)
        {
            // Log the exception details
            Console.WriteLine($"Error fetching distinct party names: {ex.Message}");
            // Return a generic 500 Internal Server Error
            return StatusCode(500, "An error occurred while fetching the list of parties.");
        }
    }
    //https://oda.ft.dk/api/Akt%C3%B8r?$inlinecount=allpages endpoint der skal bruges i hvert fald
    //----------------------------------------//
    //                To DO:                  //
    //        Overvejelser: Skal vi slette ikke aktive medlemmer? (gør processen mere ressource noget mere intensiv da vi skal tjekke hver aktør mod databasen)//
    //                                        //
    //                                        //
    //----------------------------------------//
    
    [HttpGet("fetch")]
    public async Task<IActionResult> UpdateAktorsFromExternal()
    {
        _logger.LogInformation("Starting Aktor update process..."); // Use logger

        string? initialPolitikerApiUrl = _configuration["Api:OdaApiPolitikere"];
        string? ministerTitlesApiUrl = _configuration["Api:OdaApiMinisterTitles"];
        string? ministerRelationsApiUrl = _configuration["Api:OdaApiMinisterRelationships"];

        if (string.IsNullOrEmpty(initialPolitikerApiUrl) || string.IsNullOrEmpty(ministerTitlesApiUrl) || string.IsNullOrEmpty(ministerRelationsApiUrl))
        {
            _logger.LogError("One or more required API URLs are missing in configuration.");
            return StatusCode(500, "API URL configuration is incomplete.");
        }

        var ministerTitlesMap = new Dictionary<int, string>();
        var ministerRelationshipsMap = new Dictionary<int, int>(); // Person ID -> Title ID

        try
        {
            // --- STEP 1: Fetch Ministerial Titles ---
            _logger.LogInformation("Fetching ministerial titles...");
            string? nextTitlesLink = ministerTitlesApiUrl + "&$format=json"; // Ensure JSON format
            while (!string.IsNullOrEmpty(nextTitlesLink))
            {
                var titleResponse = await _httpService.GetJsonAsync<ODataResponse<MinisterialTitleDto>>(nextTitlesLink);
                if (titleResponse?.Value != null)
                {
                    foreach (var title in titleResponse.Value)
                    {
                        if (!string.IsNullOrEmpty(title.GruppenavnKort))
                        {
                            ministerTitlesMap[title.Id] = title.GruppenavnKort;
                        }
                    }
                    _logger.LogInformation("Fetched {Count} titles from page: {Url}", titleResponse.Value.Count, nextTitlesLink);
                    nextTitlesLink = titleResponse.NextLink;
                }
                else
                {
                     _logger.LogWarning("Received null or invalid response for titles from {Url}", nextTitlesLink);
                    nextTitlesLink = null; // Stop pagination on error/null
                }
            }
            _logger.LogInformation("Finished fetching titles. Total distinct titles found: {Count}", ministerTitlesMap.Count);

            // --- STEP 2: Fetch Current Minister Relationships ---
             _logger.LogInformation("Fetching minister relationships...");
            string? nextRelationsLink = ministerRelationsApiUrl + "&$format=json"; // Ensure JSON format
            while (!string.IsNullOrEmpty(nextRelationsLink))
            {
                 var relationResponse = await _httpService.GetJsonAsync<ODataResponse<MinisterRelationshipDto>>(nextRelationsLink);
                 if (relationResponse?.Value != null)
                 {
                    foreach (var relation in relationResponse.Value)
                    {
                        // Assuming one person holds one primary minister role for simplicity.
                        // If multiple roles are possible and needed, change value to List<int>
                        ministerRelationshipsMap[relation.FraAktorId] = relation.TilAktorId;
                    }
                     _logger.LogInformation("Fetched {Count} relationships from page: {Url}", relationResponse.Value.Count, nextRelationsLink);
                    nextRelationsLink = relationResponse.NextLink;
                 }
                 else
                 {
                     _logger.LogWarning("Received null or invalid response for relationships from {Url}", nextRelationsLink);
                     nextRelationsLink = null; // Stop pagination on error/null
                 }
            }
             _logger.LogInformation("Finished fetching relationships. Total relationships found: {Count}", ministerRelationshipsMap.Count);

        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error during initial fetch of titles or relationships.");
            return StatusCode(500, $"Error fetching initial data: {ex.Message}");
        }


        // --- STEP 3: Process Politicians and Assign Titles ---
         _logger.LogInformation("Starting processing of politicians...");
        int totalAddedCount = 0;
        int totalUpdatedCount = 0;
        int totalDeletedCount = 0;
        string? nextPolitikerLink = initialPolitikerApiUrl + "&$format=json"; // Start withpoliticians URL

        while (!string.IsNullOrEmpty(nextPolitikerLink))
        {
            try
            {
                 _logger.LogDebug("Fetching politician page: {Url}", nextPolitikerLink);
                // Use JsonElement approach to handle potential OData structure variations
                var responseJson = await _httpService.GetJsonAsync<JsonElement>(nextPolitikerLink);

                if (responseJson.ValueKind == JsonValueKind.Object &&
                    responseJson.TryGetProperty("value", out var valueProperty) &&
                    valueProperty.ValueKind == JsonValueKind.Array)
                {
                    // Deserialize the 'value' array specifically
                    var externalAktors = JsonSerializer.Deserialize<List<CreateAktor>>(valueProperty.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (externalAktors != null)
                    {
                        int addedCount = 0;
                        int updatedCount = 0;
                        int deletedCount = 0;

                        foreach (var aktorDto in externalAktors)
                        {
                            var bioDetails = BioParser.ParseBiografiXml(aktorDto.biografi);
                            string? apiStatus = bioDetails.GetValueOrDefault("Status") as string;

                            var existingAktor = await _context.Aktor
                                                        .FirstOrDefaultAsync(a => a.Id == aktorDto.Id);

                            if (apiStatus == "1") // Active Politician
                            {
                                string? ministerTitle = null;
                                // Look up minister title *after* confirming the politician is active
                                if (ministerRelationshipsMap.TryGetValue(aktorDto.Id, out int titleId))
                                {
                                    ministerTitlesMap.TryGetValue(titleId, out ministerTitle);
                                }


                                if (existingAktor == null) // ADD
                                {
                                    var newAktor = MapAktor(aktorDto, bioDetails, ministerTitle); // Use helper
                                    _context.Aktor.Add(newAktor);
                                    addedCount++;
                                     _logger.LogDebug("Adding Aktor ID: {Id}, Name: {Name}, Title: {Title}", newAktor.Id, newAktor.navn, newAktor.MinisterTitel);
                                }
                                else // UPDATE
                                {
                                    MapAktor(aktorDto, bioDetails, ministerTitle, existingAktor); // Use helper to update
                                    updatedCount++;
                                     _logger.LogDebug("Updating Aktor ID: {Id}, Name: {Name}, Title: {Title}", existingAktor.Id, existingAktor.navn, existingAktor.MinisterTitel);
                                }
                            }
                            else // Inactive Politician
                            {
                                if (existingAktor != null) // DELETE
                                {
                                    _context.Aktor.Remove(existingAktor);
                                    deletedCount++;
                                     _logger.LogDebug("Deleting inactive Aktor ID: {Id}, Name: {Name}", existingAktor.Id, existingAktor.navn);
                                }
                            }
                        } // End foreach

                        if (addedCount > 0 || updatedCount > 0 || deletedCount > 0)
                        {
                             _logger.LogInformation("Saving changes for page. Added: {Added}, Updated: {Updated}, Deleted: {Deleted}", addedCount, updatedCount, deletedCount);
                            await _context.SaveChangesAsync();
                        }
                        totalAddedCount += addedCount;
                        totalUpdatedCount += updatedCount;
                        totalDeletedCount += deletedCount;

                    } // End if (externalAktors != null)
                    else {
                         _logger.LogWarning("Deserialization of 'value' array resulted in null for URL: {Url}", nextPolitikerLink);
                    }
                } else {
                     _logger.LogWarning("Response JSON was not an object or did not contain a 'value' array for URL: {Url}", nextPolitikerLink);
                }

                // Extract the next link for politicians
                if (responseJson.ValueKind == JsonValueKind.Object && responseJson.TryGetProperty("odata.nextLink", out var nextLinkProperty) && nextLinkProperty.ValueKind == JsonValueKind.String)
                {
                    nextPolitikerLink = nextLinkProperty.GetString();
                     _logger.LogDebug("Next politician page link found: {Url}", nextPolitikerLink);
                }
                else
                {
                    nextPolitikerLink = null; // No more pages
                     _logger.LogInformation("No more politician pages found.");
                }
            }
            catch (JsonException jsonEx) {
                 _logger.LogError(jsonEx, "JSON Deserialization error fetching or processing data from {Url}", nextPolitikerLink ?? "Unknown");
                nextPolitikerLink = null; // Stop pagination on error
            }
            catch (HttpRequestException httpEx){
                 _logger.LogError(httpEx, "HTTP Request error fetching data from {Url}", nextPolitikerLink ?? "Unknown");
                 nextPolitikerLink = null; // Stop pagination on error
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "General error fetching or processing data from {Url}", nextPolitikerLink ?? "Unknown");
                // Consider whether to stop pagination or try to continue
                nextPolitikerLink = null; // Stop pagination on unexpected errors
                // return StatusCode(500, $"Error processing data: {ex.Message}"); // Or handle differently
            }
        }

        _logger.LogInformation("Aktor update process finished. Total Added: {Added}, Total Updated: {Updated}, Total Deleted: {Deleted}", totalAddedCount, totalUpdatedCount, totalDeletedCount);
        return Ok($"Successfully added {totalAddedCount}, updated {totalUpdatedCount}, and deleted {totalDeletedCount} aktors.");
    }

    //map aktor fra api til objekt, fjernet fra fetch for at gøre mere overskuelig
    private Aktor MapAktor(CreateAktor dto, Dictionary<string, object> bioDetails, string? ministerTitle, Aktor? existingAktor = null)
    {
        var aktor = existingAktor ?? new Aktor(); // Create new or use existing

        aktor.Id = dto.Id; // Set ID always (PK)
        aktor.navn = dto.navn;
        aktor.fornavn = dto.fornavn;
        aktor.efternavn = dto.efternavn;
        aktor.biografi = dto.biografi;
        // Ensure DateTimeKind is set to Utc if values exist
        aktor.startdato = dto.startdato.HasValue ? DateTime.SpecifyKind(dto.startdato.Value, DateTimeKind.Utc) : null;
        aktor.slutdato = dto.slutdato.HasValue ? DateTime.SpecifyKind(dto.slutdato.Value, DateTimeKind.Utc) : null;
        aktor.opdateringsdato = DateTime.UtcNow; // Always update timestamp
        aktor.typeid = 5; // Assuming this fetch is only for politicians (typeid=5)

        // Map parsed fields
        aktor.Party = bioDetails.GetValueOrDefault("Party") as string;
        aktor.PartyShortname = bioDetails.GetValueOrDefault("PartyShortname") as string;
        aktor.Sex = bioDetails.GetValueOrDefault("Sex") as string;
        aktor.Born = bioDetails.GetValueOrDefault("Born") as string;
        aktor.EducationStatistic = bioDetails.GetValueOrDefault("EducationStatistic") as string;
        aktor.PictureMiRes = bioDetails.GetValueOrDefault("PictureMiRes") as string;
        aktor.Email = bioDetails.GetValueOrDefault("Email") as string;
        aktor.FunctionFormattedTitle = bioDetails.GetValueOrDefault("FunctionFormattedTitle") as string;
        aktor.FunctionStartDate = bioDetails.GetValueOrDefault("FunctionStartDate") as string;
        aktor.PositionsOfTrust = bioDetails.GetValueOrDefault("PositionsOfTrust") as string;

        // Map Lists (handle potential nulls from GetValueOrDefault)
        aktor.ParliamentaryPositionsOfTrust = bioDetails.GetValueOrDefault("ParliamentaryPositionsOfTrust") as List<string> ?? new List<string>();
        aktor.Constituencies = bioDetails.GetValueOrDefault("Constituencies") as List<string> ?? new List<string>();
        aktor.Nominations = bioDetails.GetValueOrDefault("Nominations") as List<string> ?? new List<string>();
        aktor.Educations = bioDetails.GetValueOrDefault("Educations") as List<string> ?? new List<string>();
        aktor.Occupations = bioDetails.GetValueOrDefault("Occupations") as List<string> ?? new List<string>();
        aktor.PublicationTitles = bioDetails.GetValueOrDefault("PublicationTitles") as List<string> ?? new List<string>();
        aktor.Ministers = bioDetails.GetValueOrDefault("Ministers") as List<string> ?? new List<string>();
        aktor.Spokesmen = bioDetails.GetValueOrDefault("Spokesmen") as List<string> ?? new List<string>();

        // Assign the fetched minister title
        aktor.MinisterTitel = ministerTitle;

        return aktor;
    }
    
}



