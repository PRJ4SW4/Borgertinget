using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using backend.Data;
using backend.DTO.FT;
using backend.Models.Politicians;
using backend.Repositories.Politicians;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace backend.Services.Politicians
{
    public class FetchService : IFetchService
    {
        private readonly HttpService _httpService;
        private readonly IConfiguration _configuration;
        private readonly IAktorRepo _aktorRepo;
        private readonly IPartyRepository _partyRepo;
        private readonly ILogger<FetchService> _logger;

        public FetchService(
            HttpService httpService,
            IConfiguration configuration,
            IAktorRepo aktorRepo,
            IPartyRepository partyRepo,
            ILogger<FetchService> logger
        )
        {
            _httpService = httpService;
            _configuration = configuration;
            _aktorRepo = aktorRepo;
            _partyRepo = partyRepo;
            _logger = logger;
        }

        public async Task<(
            int totalAdded,
            int totalUpdated,
            int totalDeleted
        )> FetchAndUpdateAktorsAsync()
        {
            _logger.LogInformation("Starting Aktor update process via AktorUpdateService...");

            string? initialPolitikerApiUrl = _configuration["Api:OdaApiPolitikere"];
            string? ministerTitlesApiUrl = _configuration["Api:OdaApiMinisterTitles"];
            string? ministerRelationsApiUrl = _configuration["Api:OdaApiMinisterRelationships"];

            if (
                string.IsNullOrEmpty(initialPolitikerApiUrl)
                || string.IsNullOrEmpty(ministerTitlesApiUrl)
                || string.IsNullOrEmpty(ministerRelationsApiUrl)
            )
            {
                _logger.LogError(
                    "One or more required API URLs are missing in configuration for AktorUpdateService."
                );
                throw new InvalidOperationException(
                    "API URL configuration for Aktor update is incomplete."
                );
            }

            var ministerTitlesMap = new Dictionary<int, string>(); //minister id til titel
            var ministerRelationshipsMap = new Dictionary<int, int>(); // Person ID -> Title ID

            try
            {
                // --- STEP 1: Fetch Ministerial Titles ---
                _logger.LogInformation("[AktorUpdateService] Fetching ministerial titles...");
                string? nextTitlesLink = ministerTitlesApiUrl + "&$format=json";
                while (!string.IsNullOrEmpty(nextTitlesLink)) // tjek alle sider af json response
                { //Henter response fra service
                    var titleResponse = await _httpService.GetJsonAsync<
                        ODataResponse<MinisterialTitleDto>
                    >(nextTitlesLink);
                    if (titleResponse?.Value != null)
                    {
                        foreach (var title in titleResponse.Value)
                        {
                            if (!string.IsNullOrEmpty(title.GruppenavnKort))
                            {
                                ministerTitlesMap[title.Id] = title.GruppenavnKort; // map ministertitel id til titelnavn
                            }
                        }
                        _logger.LogInformation(
                            "[AktorUpdateService] Fetched {Count} titles from page: {Url}",
                            titleResponse.Value.Count,
                            nextTitlesLink
                        );
                        nextTitlesLink = titleResponse.NextLink; //update link til næste side
                    }
                    else
                    {
                        _logger.LogWarning(
                            "[AktorUpdateService] Received null or invalid response for titles from {Url}",
                            nextTitlesLink
                        );
                        nextTitlesLink = null;
                    }
                }
                _logger.LogInformation(
                    "[AktorUpdateService] Finished fetching titles. Total distinct titles found: {Count}",
                    ministerTitlesMap.Count
                );

                // --- STEP 2: Fetch Current Minister Relationships ---
                _logger.LogInformation("[AktorUpdateService] Fetching minister relationships...");
                string? nextRelationsLink = ministerRelationsApiUrl + "&$format=json"; // fraAktør(politiker id) tilAktør (ministerId)
                while (!string.IsNullOrEmpty(nextRelationsLink)) //tjekker alle relatioenr
                { //hent data via service
                    var relationResponse = await _httpService.GetJsonAsync<
                        ODataResponse<MinisterRelationshipDto>
                    >(nextRelationsLink);
                    if (relationResponse?.Value != null) //vi har en value liste
                    {
                        foreach (var relation in relationResponse.Value)
                        {
                            ministerRelationshipsMap[relation.FraAktorId] = relation.TilAktorId; //map politiker id'er(key) til minister id (value)
                        }
                        _logger.LogInformation(
                            "[AktorUpdateService] Fetched {Count} relationships from page: {Url}",
                            relationResponse.Value.Count,
                            nextRelationsLink
                        );
                        nextRelationsLink = relationResponse.NextLink; //update link
                    }
                    else //no list
                    {
                        _logger.LogWarning(
                            "[AktorUpdateService] Received null or invalid response for relationships from {Url}",
                            nextRelationsLink
                        );
                        nextRelationsLink = null;
                    }
                }
                _logger.LogInformation(
                    "[AktorUpdateService] Finished fetching relationships. Total relationships found: {Count}",
                    ministerRelationshipsMap.Count
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[AktorUpdateService] Error during initial fetch of titles or relationships."
                );
                throw;
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
                    _logger.LogDebug(
                        "[AktorUpdateService] Fetching politician page: {Url}",
                        nextPolitikerLink
                    );
                    var responseJson = await _httpService.GetJsonAsync<JsonElement>(
                        nextPolitikerLink
                    );

                    if (
                        responseJson.ValueKind == JsonValueKind.Object
                        && responseJson.TryGetProperty("value", out var valueProperty)
                        && valueProperty.ValueKind == JsonValueKind.Array
                    )
                    { //Opret object med brug af dto
                        var externalAktors = JsonSerializer.Deserialize<List<CreateAktor>>(
                            valueProperty.GetRawText(),
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        if (externalAktors != null) //hvis vi har politikere at processere
                        {
                            int pageAdded = 0,
                                pageUpdated = 0,
                                pageDeleted = 0;

                            foreach (var aktorDto in externalAktors)
                            {
                                var bioDetails = BioParser.ParseBiografiXml(aktorDto.biografi); //dictionary med biografi<string, object>
                                string? activityStatus =
                                    bioDetails.GetValueOrDefault("Status") as string; //status = 1 = aktiv politiker
                                string? partyNameFromBio =
                                    bioDetails.GetValueOrDefault("Party") as string; //party
                                string? partyShortnameFromBio =
                                    bioDetails.GetValueOrDefault("PartyShortname") as string; //shortname

                                var existingAktor = await _aktorRepo.GetAktorByIdAsync(aktorDto.Id); // hent fra database

                                Aktor currentAktor; //opret objekt at arbejde med

                                if (activityStatus == "1") // Active Politician
                                {
                                    string? ministerTitle = null; //minister title at arbejde med
                                    if (
                                        ministerRelationshipsMap.TryGetValue( //tjek om politiker id findes i relations map
                                            aktorDto.Id,
                                            out int titleId
                                        )
                                    )
                                    {
                                        ministerTitlesMap.TryGetValue(titleId, out ministerTitle); //hent titel
                                    }

                                    if (existingAktor == null) //hvis politiker ikke findes i db
                                    {
                                        currentAktor = MapAktor( //map
                                            aktorDto,
                                            bioDetails,
                                            ministerTitle
                                        );
                                        await _aktorRepo.AddAktor(currentAktor); // tilføj politiker til db
                                        pageAdded++;
                                        _logger.LogDebug(
                                            "[AktorUpdateService] Adding Aktor ID: {Id}, Name: {Name}, Title: {Title}",
                                            currentAktor.Id,
                                            currentAktor.navn,
                                            currentAktor.MinisterTitel
                                        );
                                    }
                                    else //ellers updater eksisterende politiekr
                                    {
                                        currentAktor = MapAktor(
                                            aktorDto,
                                            bioDetails,
                                            ministerTitle,
                                            existingAktor
                                        );
                                        pageUpdated++;
                                        _logger.LogDebug(
                                            "[AktorUpdateService] Updating Aktor ID: {Id}, Name: {Name}, Title: {Title}",
                                            existingAktor.Id,
                                            existingAktor.navn,
                                            existingAktor.MinisterTitel
                                        );
                                    }

                                    // Link Aktor.Id to PoliticianTwitterId.AktorId based on matching names
                                    if (
                                        currentAktor != null // Add null check for currentAktor
                                        && !string.IsNullOrWhiteSpace(currentAktor.navn)
                                    )
                                    {
                                        var politicianTwitterEntry =
                                            await _aktorRepo.GetPoliticianTwitterIdByNameAsync(
                                                currentAktor.navn
                                            );

                                        if (politicianTwitterEntry != null)
                                        {
                                            // Check if an update is needed to avoid unnecessary database operations/logging
                                            if (politicianTwitterEntry.AktorId != currentAktor.Id)
                                            {
                                                politicianTwitterEntry.AktorId = currentAktor.Id;
                                                _logger.LogInformation(
                                                    $"Updated PoliticianTwitterId.AktorId for '{politicianTwitterEntry.Name}' (PoliticianTwitterId: {politicianTwitterEntry.Id}) to Aktor ID: {currentAktor.Id}."
                                                );
                                            }
                                        }
                                        else
                                        {
                                            _logger.LogWarning(
                                                $"No PoliticianTwitterId record found with Name '{currentAktor.navn}' to link with Aktor ID {currentAktor.Id}."
                                            );
                                        }
                                    }

                                    if (!string.IsNullOrWhiteSpace(partyNameFromBio)) // til at oprette partier
                                    {
                                        Party? partyEnt; // opret party entity
                                        if (
                                            !processedParties.TryGetValue(
                                                partyNameFromBio,
                                                out partyEnt
                                            )
                                        )
                                        {
                                            partyEnt = await _partyRepo.GetByName(partyNameFromBio); // hent parti med matchende navn
                                            if (partyEnt == null) //vi har ikke partiet
                                            {
                                                partyEnt = new Party //nyt party
                                                {
                                                    partyName = partyNameFromBio,
                                                    partyShortName = partyShortnameFromBio,
                                                    memberIds = new List<int>(),
                                                };
                                                await _partyRepo.AddParty(partyEnt); // tilføj parti til database
                                            }
                                            partyEnt.memberIds ??= new List<int>(); // Ensure list is initialized
                                            processedParties[partyNameFromBio] = partyEnt; //logging
                                        }
                                        if (!partyEnt.memberIds!.Contains(currentAktor!.Id)) //tjek om politiker allerede findes i partiet, hvis ikke tilføj id
                                        {
                                            partyEnt.memberIds.Add(currentAktor.Id);
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogWarning(
                                            "[AktorUpdateService] Aktor ID: {Id} has no party name in biography.",
                                            currentAktor!.Id
                                        );
                                    }
                                }
                                else // Inactive Politician
                                {
                                    if (existingAktor != null) //vi skal slette medlem
                                    {
                                        var partiesContainingAktor =
                                            await _partyRepo.GetPartyByMemberId(existingAktor.Id);

                                        foreach (var party in partiesContainingAktor) //redundant tjek om politiker af en eller anden grund skulle findes i flere partier
                                        {
                                            await _partyRepo.RemoveMember(party, existingAktor.Id); //slet politiker id fra alle partiers medlems liste
                                        }
                                        await _aktorRepo.DeleteAktor(existingAktor); //slet politiker fra db
                                        pageDeleted++;
                                        _logger.LogDebug(
                                            "[AktorUpdateService] Deleting Aktor ID: {Id}, Name: {Name}",
                                            existingAktor.Id,
                                            existingAktor.navn
                                        );
                                    }
                                }
                            }
                            totalAddedCount += pageAdded;
                            totalUpdatedCount += pageUpdated;
                            totalDeletedCount += pageDeleted;
                            _logger.LogInformation(
                                "[AktorUpdateService] Processed page. Added: {Added}, Updated: {Updated}, Deleted: {Deleted}",
                                pageAdded,
                                pageUpdated,
                                pageDeleted
                            );
                        }
                        else
                        {
                            _logger.LogWarning(
                                "[AktorUpdateService] Deserialization of 'value' array resulted in null for URL: {Url}",
                                nextPolitikerLink
                            );
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "[AktorUpdateService] Response JSON was not an object or did not contain a 'value' array for URL: {Url}",
                            nextPolitikerLink
                        );
                    }

                    if (
                        responseJson.ValueKind == JsonValueKind.Object
                        && responseJson.TryGetProperty("odata.nextLink", out var nextLinkProperty)
                        && nextLinkProperty.ValueKind == JsonValueKind.String
                    )
                    {
                        nextPolitikerLink = nextLinkProperty.GetString();
                        _logger.LogDebug(
                            "[AktorUpdateService] Next politician page link found: {Url}",
                            nextPolitikerLink
                        );
                    }
                    else
                    {
                        nextPolitikerLink = null;
                        _logger.LogInformation(
                            "[AktorUpdateService] No more politician pages found."
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "[AktorUpdateService] Error fetching or processing data from {Url}",
                        nextPolitikerLink ?? "Unknown"
                    );
                    nextPolitikerLink = null;
                }
            }
            await _aktorRepo.SaveChangesAsync(); //gem alle ændringer
            _logger.LogInformation(
                "[AktorUpdateService] Aktor update process finished. Total Added: {Added}, Total Updated: {Updated}, Total Deleted: {Deleted}",
                totalAddedCount,
                totalUpdatedCount,
                totalDeletedCount
            );
            return (totalAddedCount, totalUpdatedCount, totalDeletedCount);
        }

        private Aktor MapAktor(
            CreateAktor dto,
            Dictionary<string, object> bioDetails,
            string? ministerTitle,
            Aktor? existingAktor = null
        )
        {
            var aktor = existingAktor ?? new Aktor();

            aktor.Id = dto.Id;
            aktor.navn = dto.navn;
            aktor.fornavn = dto.fornavn;
            aktor.efternavn = dto.efternavn;
            aktor.biografi = dto.biografi;

            aktor.startdato = dto.startdato.HasValue
                ? DateTime.SpecifyKind(dto.startdato.Value, DateTimeKind.Utc)
                : null;
            aktor.slutdato = dto.slutdato.HasValue
                ? DateTime.SpecifyKind(dto.slutdato.Value, DateTimeKind.Utc)
                : null;
            aktor.opdateringsdato = DateTime.UtcNow;
            aktor.typeid = 5;

            // Map parsed fields from BioParser
            aktor.Party = bioDetails.GetValueOrDefault("Party") as string;
            aktor.PartyShortname = bioDetails.GetValueOrDefault("PartyShortname") as string;
            aktor.Sex = bioDetails.GetValueOrDefault("Sex") as string;
            aktor.Born = bioDetails.GetValueOrDefault("Born") as string;
            aktor.EducationStatistic = bioDetails.GetValueOrDefault("EducationStatistic") as string;
            aktor.PictureMiRes = bioDetails.GetValueOrDefault("PictureMiRes") as string;
            aktor.Email = bioDetails.GetValueOrDefault("Email") as string;
            aktor.FunctionFormattedTitle =
                bioDetails.GetValueOrDefault("FunctionFormattedTitle") as string;
            aktor.FunctionStartDate = bioDetails.GetValueOrDefault("FunctionStartDate") as string;
            aktor.PositionsOfTrust = bioDetails.GetValueOrDefault("PositionsOfTrust") as string;
            aktor.MinisterTitel = ministerTitle;

            aktor.ParliamentaryPositionsOfTrust =
                bioDetails.GetValueOrDefault("ParliamentaryPositionsOfTrust") as List<string>
                ?? new List<string>();
            aktor.Constituencies =
                bioDetails.GetValueOrDefault("Constituencies") as List<string>
                ?? new List<string>();
            aktor.Nominations =
                bioDetails.GetValueOrDefault("Nominations") as List<string> ?? new List<string>();
            aktor.Educations =
                bioDetails.GetValueOrDefault("Educations") as List<string> ?? new List<string>();
            aktor.Occupations =
                bioDetails.GetValueOrDefault("Occupations") as List<string> ?? new List<string>();
            aktor.PublicationTitles =
                bioDetails.GetValueOrDefault("PublicationTitles") as List<string>
                ?? new List<string>();
            aktor.Ministers =
                bioDetails.GetValueOrDefault("Ministers") as List<string> ?? new List<string>();
            aktor.Spokesmen =
                bioDetails.GetValueOrDefault("Spokesmen") as List<string> ?? new List<string>();

            return aktor;
        }
    }
}
