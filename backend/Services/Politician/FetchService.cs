using backend.Data;
using backend.DTO.FT;
using backend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Politician{
    public class FetchService : IFetchService
    {
        private readonly DataContext _context;
        private readonly HttpService _httpService; // Assuming HttpService is still used
        private readonly IConfiguration _configuration;
        private readonly ILogger<FetchService> _logger;

        public FetchService(
            DataContext context,
            HttpService httpService,
            IConfiguration configuration,
            ILogger<FetchService> logger)
        {
            _context = context;
            _httpService = httpService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(int totalAdded, int totalUpdated, int totalDeleted)> FetchAndUpdateAktorsAsync()
        {
            _logger.LogInformation("Starting Aktor update process via AktorUpdateService...");

            string? initialPolitikerApiUrl = _configuration["Api:OdaApiPolitikere"];
            string? ministerTitlesApiUrl = _configuration["Api:OdaApiMinisterTitles"];
            string? ministerRelationsApiUrl = _configuration["Api:OdaApiMinisterRelationships"];

            if (string.IsNullOrEmpty(initialPolitikerApiUrl) || string.IsNullOrEmpty(ministerTitlesApiUrl) || string.IsNullOrEmpty(ministerRelationsApiUrl))
            {
                _logger.LogError("One or more required API URLs are missing in configuration for AktorUpdateService.");
                // Consider throwing a specific exception or returning a failure indicator
                throw new InvalidOperationException("API URL configuration for Aktor update is incomplete.");
            }

            var ministerTitlesMap = new Dictionary<int, string>();
            var ministerRelationshipsMap = new Dictionary<int, int>(); // Person ID -> Title ID

            try
            {
                // --- STEP 1: Fetch Ministerial Titles ---
                _logger.LogInformation("[AktorUpdateService] Fetching ministerial titles...");
                string? nextTitlesLink = ministerTitlesApiUrl + "&$format=json";
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
                        _logger.LogInformation("[AktorUpdateService] Fetched {Count} titles from page: {Url}", titleResponse.Value.Count, nextTitlesLink);
                        nextTitlesLink = titleResponse.NextLink;
                    }
                    else
                    {
                        _logger.LogWarning("[AktorUpdateService] Received null or invalid response for titles from {Url}", nextTitlesLink);
                        nextTitlesLink = null;
                    }
                }
                _logger.LogInformation("[AktorUpdateService] Finished fetching titles. Total distinct titles found: {Count}", ministerTitlesMap.Count);

                // --- STEP 2: Fetch Current Minister Relationships ---
                _logger.LogInformation("[AktorUpdateService] Fetching minister relationships...");
                string? nextRelationsLink = ministerRelationsApiUrl + "&$format=json";
                while (!string.IsNullOrEmpty(nextRelationsLink))
                {
                    var relationResponse = await _httpService.GetJsonAsync<ODataResponse<MinisterRelationshipDto>>(nextRelationsLink);
                    if (relationResponse?.Value != null)
                    {
                        foreach (var relation in relationResponse.Value)
                        {
                            ministerRelationshipsMap[relation.FraAktorId] = relation.TilAktorId;
                        }
                        _logger.LogInformation("[AktorUpdateService] Fetched {Count} relationships from page: {Url}", relationResponse.Value.Count, nextRelationsLink);
                        nextRelationsLink = relationResponse.NextLink;
                    }
                    else
                    {
                        _logger.LogWarning("[AktorUpdateService] Received null or invalid response for relationships from {Url}", nextRelationsLink);
                        nextRelationsLink = null;
                    }
                }
                _logger.LogInformation("[AktorUpdateService] Finished fetching relationships. Total relationships found: {Count}", ministerRelationshipsMap.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AktorUpdateService] Error during initial fetch of titles or relationships.");
                throw; // Re-throw to be handled by the controller or a higher-level error handler
            }

            // --- STEP 3: Process Politicians and Assign Titles ---
            _logger.LogInformation("[AktorUpdateService] Starting processing of politicians...");
            int totalAddedCount = 0;
            int totalUpdatedCount = 0;
            int totalDeletedCount = 0;
            string? nextPolitikerLink = initialPolitikerApiUrl + "&$format=json";
            var processedParties = new Dictionary<string, Party>();

            while (!string.IsNullOrEmpty(nextPolitikerLink))
            {
                try
                {
                    _logger.LogDebug("[AktorUpdateService] Fetching politician page: {Url}", nextPolitikerLink);
                    var responseJson = await _httpService.GetJsonAsync<JsonElement>(nextPolitikerLink);

                    if (responseJson.ValueKind == JsonValueKind.Object &&
                        responseJson.TryGetProperty("value", out var valueProperty) &&
                        valueProperty.ValueKind == JsonValueKind.Array)
                    {
                        var externalAktors = JsonSerializer.Deserialize<List<CreateAktor>>(valueProperty.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (externalAktors != null)
                        {
                            int pageAdded = 0, pageUpdated = 0, pageDeleted = 0;

                            foreach (var aktorDto in externalAktors)
                            {
                                var bioDetails = BioParser.ParseBiografiXml(aktorDto.biografi);
                                string? apiStatus = bioDetails.GetValueOrDefault("Status") as string;
                                string? partyNameFromBio = bioDetails.GetValueOrDefault("Party") as string;
                                string? partyShortnameFromBio = bioDetails.GetValueOrDefault("PartyShortname") as string;

                                var existingAktor = await _context.Aktor.FirstOrDefaultAsync(a => a.Id == aktorDto.Id);
                                Aktor currentAktor;

                                if (apiStatus == "1") // Active Politician
                                {
                                    string? ministerTitle = null;
                                    if (ministerRelationshipsMap.TryGetValue(aktorDto.Id, out int titleId))
                                    {
                                        ministerTitlesMap.TryGetValue(titleId, out ministerTitle);
                                    }

                                    if (existingAktor == null)
                                    {
                                        currentAktor = MapAktor(aktorDto, bioDetails, ministerTitle);
                                        _context.Aktor.Add(currentAktor);
                                        pageAdded++;
                                        _logger.LogDebug("[AktorUpdateService] Adding Aktor ID: {Id}, Name: {Name}, Title: {Title}", currentAktor.Id, currentAktor.navn, currentAktor.MinisterTitel);
                                    }
                                    else
                                    {
                                        currentAktor = MapAktor(aktorDto, bioDetails, ministerTitle, existingAktor);
                                        pageUpdated++;
                                         _logger.LogDebug("[AktorUpdateService] Updating Aktor ID: {Id}, Name: {Name}, Title: {Title}", existingAktor.Id, existingAktor.navn, existingAktor.MinisterTitel);
                                    }

                                    if (!string.IsNullOrWhiteSpace(partyNameFromBio))
                                    {
                                        Party? partyEnt;
                                        if (!processedParties.TryGetValue(partyNameFromBio, out partyEnt))
                                        {
                                            partyEnt = await _context.Party.FirstOrDefaultAsync(p => p.partyName == partyNameFromBio);
                                            if (partyEnt == null)
                                            {
                                                partyEnt = new Party {
                                                    partyName = partyNameFromBio,
                                                    partyShortName = partyShortnameFromBio,
                                                    memberIds = new List<int>()
                                                };
                                                _context.Party.Add(partyEnt);
                                            }
                                            partyEnt.memberIds ??= new List<int>(); // Ensure list is initialized
                                            processedParties[partyNameFromBio] = partyEnt;
                                        }
                                        if (!partyEnt.memberIds.Contains(currentAktor.Id))
                                        {
                                            partyEnt.memberIds.Add(currentAktor.Id);
                                        }
                                    } else {
                                         _logger.LogWarning("[AktorUpdateService] Aktor ID: {Id} has no party name in biography.", currentAktor.Id);
                                    }
                                }
                                else // Inactive Politician
                                {
                                    if (existingAktor != null)
                                    {
                                        var partiesContainingAktor = await _context.Party
                                            .Where(p => p.memberIds != null && p.memberIds.Contains(existingAktor.Id))
                                            .ToListAsync();
                                        foreach(var party in partiesContainingAktor)
                                        {
                                            party.memberIds?.Remove(existingAktor.Id);
                                             _context.Entry(party).State = EntityState.Modified;
                                        }
                                        _context.Aktor.Remove(existingAktor);
                                        pageDeleted++;
                                         _logger.LogDebug("[AktorUpdateService] Deleting Aktor ID: {Id}, Name: {Name}", existingAktor.Id, existingAktor.navn);
                                    }
                                }
                            }
                            totalAddedCount += pageAdded;
                            totalUpdatedCount += pageUpdated;
                            totalDeletedCount += pageDeleted;
                            _logger.LogInformation("[AktorUpdateService] Processed page. Added: {Added}, Updated: {Updated}, Deleted: {Deleted}", pageAdded, pageUpdated, pageDeleted);
                        }
                        else {
                            _logger.LogWarning("[AktorUpdateService] Deserialization of 'value' array resulted in null for URL: {Url}", nextPolitikerLink);
                        }
                    } else {
                         _logger.LogWarning("[AktorUpdateService] Response JSON was not an object or did not contain a 'value' array for URL: {Url}", nextPolitikerLink);
                    }

                    if (responseJson.ValueKind == JsonValueKind.Object && responseJson.TryGetProperty("odata.nextLink", out var nextLinkProperty) && nextLinkProperty.ValueKind == JsonValueKind.String)
                    {
                        nextPolitikerLink = nextLinkProperty.GetString();
                        _logger.LogDebug("[AktorUpdateService] Next politician page link found: {Url}", nextPolitikerLink);
                    }
                    else
                    {
                        nextPolitikerLink = null;
                        _logger.LogInformation("[AktorUpdateService] No more politician pages found.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[AktorUpdateService] Error fetching or processing data from {Url}", nextPolitikerLink ?? "Unknown");
                    // Decide if you want to stop the whole process on a page error or continue
                    // For now, we'll log and continue to the next link if one was previously found,
                    // or stop if this was the first page error.
                    // To be safer, perhaps set nextPolitikerLink to null to stop further processing.
                    nextPolitikerLink = null; // Stop on error for this page.
                }
            }
            await _context.SaveChangesAsync(); // Save all accumulated changes
            _logger.LogInformation("[AktorUpdateService] Aktor update process finished. Total Added: {Added}, Total Updated: {Updated}, Total Deleted: {Deleted}", totalAddedCount, totalUpdatedCount, totalDeletedCount);
            return (totalAddedCount, totalUpdatedCount, totalDeletedCount);
        }

        private Aktor MapAktor(CreateAktor dto, Dictionary<string, object> bioDetails, string? ministerTitle, Aktor? existingAktor = null)
        {
            var aktor = existingAktor ?? new Aktor();

            aktor.Id = dto.Id;
            aktor.navn = dto.navn;
            aktor.fornavn = dto.fornavn;
            aktor.efternavn = dto.efternavn;
            aktor.biografi = dto.biografi; // Store the raw XML biography

            aktor.startdato = dto.startdato.HasValue ? DateTime.SpecifyKind(dto.startdato.Value, DateTimeKind.Utc) : null;
            aktor.slutdato = dto.slutdato.HasValue ? DateTime.SpecifyKind(dto.slutdato.Value, DateTimeKind.Utc) : null;
            aktor.opdateringsdato = DateTime.UtcNow;
            aktor.typeid = 5; // Default to 5 for person

            // Map parsed fields from BioParser
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
            aktor.MinisterTitel = ministerTitle; // Assign the looked-up title

            aktor.ParliamentaryPositionsOfTrust = bioDetails.GetValueOrDefault("ParliamentaryPositionsOfTrust") as List<string> ?? new List<string>();
            aktor.Constituencies = bioDetails.GetValueOrDefault("Constituencies") as List<string> ?? new List<string>();
            aktor.Nominations = bioDetails.GetValueOrDefault("Nominations") as List<string> ?? new List<string>();
            aktor.Educations = bioDetails.GetValueOrDefault("Educations") as List<string> ?? new List<string>();
            aktor.Occupations = bioDetails.GetValueOrDefault("Occupations") as List<string> ?? new List<string>();
            aktor.PublicationTitles = bioDetails.GetValueOrDefault("PublicationTitles") as List<string> ?? new List<string>();
            aktor.Ministers = bioDetails.GetValueOrDefault("Ministers") as List<string> ?? new List<string>();
            aktor.Spokesmen = bioDetails.GetValueOrDefault("Spokesmen") as List<string> ?? new List<string>();

            return aktor;
        }
    }
}
