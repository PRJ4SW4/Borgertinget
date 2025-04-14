
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using backend.Services;
using backend.Models; // Namespace containing Provider model
using backend.Data; // Namespace for your DbContext (e.g., AppDbContext)
using backend.DTO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;


[ApiController]
[Route("api/[controller]")]
public class AktorController : ControllerBase{
    private readonly DataContext _context;
    private readonly HttpService _httpService;
    private readonly IConfiguration _configuration;

    public AktorController(DataContext context, HttpService httpService, IConfiguration conf){
        _context = context;
        _httpService = httpService;
        _configuration = conf;
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<Aktor>>> GetAllAktors(){
        var aktors = await _context.Aktor.ToListAsync();

        return Ok(aktors);
    }
    [HttpGet("{id}")] // Route definition: api/Aktor/{id} (e.g., api/Aktor/123)
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

            // Return the found politician with 200 OK status
            return Ok(aktor);
        }
        catch (Exception ex)
        {
            // Log the exception details (consider using a proper logging framework)
            Console.WriteLine($"Error fetching politician with ID {id}: {ex.Message}");
            // Return a generic 500 Internal Server Error
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

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
                .ToListAsync();

            // Check if any politicians were found
            if (filteredPoliticians == null || !filteredPoliticians.Any())
            {
                // Option 1: Return 404 Not Found if no matches
                // return NotFound($"No politicians found for party '{partyName}'.");

                // Option 2: Return 200 OK with an empty list (often preferred for collections)
                return Ok(new List<Aktor>());
            }

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
        
        string? initialApiUrl = _configuration["Api:OdaApi"];

        if (string.IsNullOrEmpty(initialApiUrl))
        {
            return StatusCode(500 ,"Initial external API URL is required.");
        }


        int totalAddedCount = 0;
        int totalUpdatedCount = 0;
        int totalDeletedCount = 0;
        string? nextLink = initialApiUrl;

        while (!string.IsNullOrEmpty(nextLink))
        {
            try
            {
                var responseJson = await _httpService.GetJsonAsync<JsonElement>(nextLink);

                if (responseJson.ValueKind == JsonValueKind.Object &&
                    responseJson.TryGetProperty("value", out var valueProperty) &&
                    valueProperty.ValueKind == JsonValueKind.Array)
                {
                var externalAktors = JsonSerializer.Deserialize<List<CreateAktor>>(valueProperty.GetRawText());

                if (externalAktors != null)
                    {
                        int addedCount = 0;
                        int updatedCount = 0;
                        int deletedCount = 0; // <-- Add counter for deletions

                        foreach (var aktorDto in externalAktors)
                        {
                            //Parse biografi details first for every record
                            var bioDetails = BioParser.ParseBiografiXml(aktorDto.biografi);
                            string? apiStatus = bioDetails.GetValueOrDefault("Status") as string;

                            // Look for aktor in DB with same ID
                            var existingAktor = await _context.Aktor
                                                        .FirstOrDefaultAsync(a => a.Id == aktorDto.Id);

                            //Decide action based on API status and local existence
                            if (apiStatus == "1") // --- Politician is ACTIVE in API data ---
                            {
                                if (existingAktor == null) // Active in API, NOT in DB -> ADD
                                {
                                    var newAktor = new Aktor
                                    {
                                        
                                        Id = aktorDto.Id,
                                        navn = aktorDto.navn,
                                        fornavn = aktorDto.fornavn,
                                        efternavn = aktorDto.efternavn,
                                        biografi = aktorDto.biografi,
                                        startdato = aktorDto.startdato.HasValue ? DateTime.SpecifyKind(aktorDto.startdato.Value, DateTimeKind.Utc) : null,
                                        slutdato = aktorDto.slutdato.HasValue ? DateTime.SpecifyKind(aktorDto.slutdato.Value, DateTimeKind.Utc) : null,
                                        opdateringsdato = DateTime.UtcNow,

                                        // Parsed fields
                                        Party = bioDetails.GetValueOrDefault("Party") as string,
                                        PartyShortname = bioDetails.GetValueOrDefault("PartyShortname") as string,
                                        Sex = bioDetails.GetValueOrDefault("Sex") as string,
                                        Born = bioDetails.GetValueOrDefault("Born") as string,
                                        EducationStatistic = bioDetails.GetValueOrDefault("EducationStatistic") as string,
                                        PictureMiRes = bioDetails.GetValueOrDefault("PictureMiRes") as string,
                                        Email = bioDetails.GetValueOrDefault("Email") as string,
                                        FunctionFormattedTitle = bioDetails.GetValueOrDefault("FunctionFormattedTitle") as string,
                                        FunctionStartDate = bioDetails.GetValueOrDefault("FunctionStartDate") as string,
                                        PositionsOfTrust = bioDetails.GetValueOrDefault("PositionsOfTrust") as string,
                                        ParliamentaryPositionsOfTrust = bioDetails.GetValueOrDefault("ParliamentaryPositionsOfTrust") as List<string>,
                                        Constituencies = bioDetails.GetValueOrDefault("Constituencies") as List<string>,
                                        Nominations = bioDetails.GetValueOrDefault("Nominations") as List<string>,
                                        Educations = bioDetails.GetValueOrDefault("Educations") as List<string>,
                                        Occupations = bioDetails.GetValueOrDefault("Occupations") as List<string>,
                                        PublicationTitles = bioDetails.GetValueOrDefault("PublicationTitles") as List<string>,
                                        // Add other parsed fields: Ministers, Spokesmen if needed
                                        Ministers = bioDetails.GetValueOrDefault("Ministers") as List<string>,
                                        Spokesmen = bioDetails.GetValueOrDefault("Spokesmen") as List<string>
                                    };
                                    _context.Aktor.Add(newAktor);
                                    addedCount++;
                                }
                                else // Active in API, EXISTS in DB -> UPDATE
                                {
                                    // Update properties of existingAktor
                                    existingAktor.navn = aktorDto.navn;
                                    existingAktor.fornavn = aktorDto.fornavn;
                                    existingAktor.efternavn = aktorDto.efternavn;
                                    existingAktor.biografi = aktorDto.biografi;
                                    existingAktor.startdato = aktorDto.startdato.HasValue ? DateTime.SpecifyKind(aktorDto.startdato.Value, DateTimeKind.Utc) : null;
                                    existingAktor.slutdato = aktorDto.slutdato.HasValue ? DateTime.SpecifyKind(aktorDto.slutdato.Value, DateTimeKind.Utc) : null;
                                    existingAktor.opdateringsdato = DateTime.UtcNow;
                                    // Update parsed fields
                                    existingAktor.Party = bioDetails.GetValueOrDefault("Party") as string;
                                    existingAktor.PartyShortname = bioDetails.GetValueOrDefault("PartyShortname") as string;
                                    existingAktor.Sex = bioDetails.GetValueOrDefault("Sex") as string;
                                    existingAktor.Born = bioDetails.GetValueOrDefault("Born") as string;
                                    existingAktor.EducationStatistic = bioDetails.GetValueOrDefault("EducationStatistic") as string;
                                    existingAktor.PictureMiRes = bioDetails.GetValueOrDefault("PictureMiRes") as string;
                                    existingAktor.Email = bioDetails.GetValueOrDefault("Email") as string;
                                    existingAktor.FunctionFormattedTitle = bioDetails.GetValueOrDefault("FunctionFormattedTitle") as string;
                                    existingAktor.FunctionStartDate = bioDetails.GetValueOrDefault("FunctionStartDate") as string;
                                    existingAktor.PositionsOfTrust = bioDetails.GetValueOrDefault("PositionsOfTrust") as string;
                                    existingAktor.ParliamentaryPositionsOfTrust = bioDetails.GetValueOrDefault("ParliamentaryPositionsOfTrust") as List<string>;
                                    existingAktor.Constituencies = bioDetails.GetValueOrDefault("Constituencies") as List<string>;
                                    existingAktor.Nominations = bioDetails.GetValueOrDefault("Nominations") as List<string>;
                                    existingAktor.Educations = bioDetails.GetValueOrDefault("Educations") as List<string>;
                                    existingAktor.Occupations = bioDetails.GetValueOrDefault("Occupations") as List<string>;
                                    existingAktor.PublicationTitles = bioDetails.GetValueOrDefault("PublicationTitles") as List<string>;
                                    existingAktor.Ministers = bioDetails.GetValueOrDefault("Ministers") as List<string>;
                                    existingAktor.Spokesmen = bioDetails.GetValueOrDefault("Spokesmen") as List<string>;

                                    updatedCount++;
                                }
                            }
                            else // --- Politician is INACTIVE (Status != "1") in API data ---
                            {
                                if (existingAktor != null) // Inactive in API, EXISTS in DB -> DELETE
                                {
                                    _context.Aktor.Remove(existingAktor); // Remove the existing record
                                    deletedCount++; // Increment deleted counter
                                }
                                // Else (Inactive in API, NOT in DB): Do nothing, already absent.
                            }
                        } // End foreach

                        // Save changes if any adds, updates, OR deletes occurred
                        if (addedCount > 0 || updatedCount > 0 || deletedCount > 0) // <-- Add deletedCount check
                        {
                            Console.WriteLine($"Saving changes for page. Added: {addedCount}, Updated: {updatedCount}, Deleted: {deletedCount}"); // Enhanced log
                            await _context.SaveChangesAsync();
                        }
                        totalAddedCount += addedCount;
                        totalUpdatedCount += updatedCount;
                        // Keep track of total deleted // Declare this outside the 'while' loop
                        totalDeletedCount += deletedCount; // Add this line here

                    } // End if (externalAktors != null)
                }

                // Extract the next link
                if (responseJson.TryGetProperty("odata.nextLink", out var nextLinkProperty) && nextLinkProperty.ValueKind == JsonValueKind.String)
                {
                    nextLink = nextLinkProperty.GetString();
                    Console.WriteLine($"Fetching next page: {nextLink}"); // Optional logging
                }
                else
                {
                    nextLink = null; // No more pages
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching or processing data from {nextLink}: {ex.Message}");
            }
        }

        return Ok($"Successfully added {totalAddedCount}, updated {totalUpdatedCount} and deleted {totalDeletedCount} aktors from the external API.");
    }
}



