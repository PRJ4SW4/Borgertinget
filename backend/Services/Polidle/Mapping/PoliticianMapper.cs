// Fil: Services/Mapping/PoliticianMapper.cs
using backend.DTO;
using backend.Interfaces.Services;
using backend.Interfaces.Utility; // For IDateTimeProvider
using backend.Models;
using Microsoft.Extensions.Logging; // For logging
using System;
using System.Collections.Generic;
using System.Globalization; // For CultureInfo
using System.Linq;

namespace backend.Services.Mapping
{
    public class PoliticianMapper : IPoliticianMapper
    {
         private readonly ILogger<PoliticianMapper> _logger;
         private readonly IDateTimeProvider _dateTimeProvider; // Til at få dags dato for alder


         public PoliticianMapper(ILogger<PoliticianMapper> logger, IDateTimeProvider dateTimeProvider)
         {
             _logger = logger;
             _dateTimeProvider = dateTimeProvider;
         }

         public DailyPoliticianDto MapToDetailsDto(Aktor aktor, DateOnly referenceDate) // ReferenceDate kan evt. fjernes hvis IDateTimeProvider altid bruges
         {
             if (aktor == null) throw new ArgumentNullException(nameof(aktor));

             // Her sker simplificering/mapping - SKAL VEDLIGEHOLDES!
             string? region = aktor.Constituencies?.FirstOrDefault(); // Tag første valgkreds
             string? uddannelse = aktor.Educations?.FirstOrDefault() ?? aktor.EducationStatistic; // Tag første udd. eller statistik

             // Use PartyShortname if available, otherwise fallback to full Party name, then "Ukendt Parti"
            string partyDisplayValue = !string.IsNullOrWhiteSpace(aktor.PartyShortname) 
                                        ? aktor.PartyShortname 
                                        : (!string.IsNullOrWhiteSpace(aktor.Party) ? aktor.Party : "Ukendt Parti");


             int age = CalculateAge(aktor.Born, _dateTimeProvider.TodayUtc); // Brug IDateTimeProvider


             var dto = new DailyPoliticianDto
             {
                 Id = aktor.Id,
                 PolitikerNavn = aktor.navn ?? "N/A",
                 PictureUrl = aktor.PictureMiRes,
                 Køn = aktor.Sex,
                 PartyShortname = partyDisplayValue,
                 Age = age,
                 Region = region,
                 Uddannelse = uddannelse,
             };

             _logger.LogDebug("Mapped Aktor {AktorId} to PoliticianDetailsDto.", aktor.Id);
             return dto;
         }

         // Overload for nemheds skyld, bruger TodayUtc fra provider
         public DailyPoliticianDto MapToDetailsDto(Aktor aktor) => MapToDetailsDto(aktor, _dateTimeProvider.TodayUtc);

         public List<SearchListDto> MapToSummaryDtoList(IEnumerable<Aktor> aktors)
         {
              if (aktors == null) return new List<SearchListDto>();
              return aktors.Select(MapToSummaryDto).ToList();
         }

         public SearchListDto MapToSummaryDto(Aktor aktor)
         {
              if (aktor == null) throw new ArgumentNullException(nameof(aktor));
              return new SearchListDto
              {
                  Id = aktor.Id,
                  PolitikerNavn = aktor.navn ?? "N/A",
                  PictureUrl = aktor.PictureMiRes // Antager DTO bruger PictureUrl
              };
         }

          // --- Privat Age Calculation ---
          // (Kan også flyttes til en statisk DateUtils klasse)
          private int CalculateAge(string? dateOfBirthString, DateOnly referenceDate)
          {
            if (string.IsNullOrEmpty(dateOfBirthString)){
                return 0;
            }

            try{
                if(DateTime.TryParse(dateOfBirthString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces, out var dobDateTime)){
                    DateOnly dateOfBirth = DateOnly.FromDateTime(dobDateTime);

                    int age = referenceDate.Year - dateOfBirth.Year;

                    if(referenceDate.Year < dateOfBirth.DayOfYear){
                        --age;
                    }

                    return Math.Max(0, age);
                } else{
                    return 0;
                }
            }catch(Exception ex){
                return 0;
            }
          }
    }
}