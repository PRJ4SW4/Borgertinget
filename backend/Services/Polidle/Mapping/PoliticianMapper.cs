using System.Globalization;
using backend.DTO;
using backend.Interfaces.Services;
using backend.Interfaces.Utility;
using backend.Models.Politicians;

namespace backend.Services.Mapping
{
    public class PoliticianMapper : IPoliticianMapper
    {
        private readonly ILogger<PoliticianMapper> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;

        public PoliticianMapper(
            ILogger<PoliticianMapper> logger,
            IDateTimeProvider dateTimeProvider
        )
        {
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
        }

        public DailyPoliticianDto MapToDetailsDto(Aktor? aktor, DateOnly referenceDate)
        {
            if (aktor == null)
                throw new ArgumentNullException(nameof(aktor));

            string? region = aktor.Constituencies?.FirstOrDefault();
            string? uddannelse = aktor.Educations?.FirstOrDefault() ?? aktor.EducationStatistic;

            string partyDisplayValue = !string.IsNullOrWhiteSpace(aktor.PartyShortname)
                ? aktor.PartyShortname
                : (!string.IsNullOrWhiteSpace(aktor.Party) ? aktor.Party : "Ukendt Parti");

            int age = CalculateAge(aktor.Born, _dateTimeProvider.TodayUtc);

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

        public DailyPoliticianDto MapToDetailsDto(Aktor aktor) =>
            MapToDetailsDto(aktor, _dateTimeProvider.TodayUtc);

        public List<SearchListDto> MapToSummaryDtoList(IEnumerable<Aktor>? aktors)
        {
            if (aktors == null)
                return new List<SearchListDto>();
            return aktors.Select(MapToSummaryDto).ToList();
        }

        public SearchListDto MapToSummaryDto(Aktor? aktor)
        {
            if (aktor == null)
                throw new ArgumentNullException(nameof(aktor));
            return new SearchListDto
            {
                Id = aktor.Id,
                PolitikerNavn = aktor.navn ?? "N/A",
                PictureUrl = aktor.PictureMiRes,
            };
        }

        // --- Privat Age Calculation ---
        private int CalculateAge(string? dateOfBirthString, DateOnly referenceDate)
        {
            if (string.IsNullOrEmpty(dateOfBirthString))
            {
                return 0;
            }

            try
            {
                if (
                    DateTime.TryParseExact(
                        dateOfBirthString,
                        "dd-MM-yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var dobDateTime
                    )
                )
                {
                    DateOnly dateOfBirth = DateOnly.FromDateTime(dobDateTime);

                    int age = referenceDate.Year - dateOfBirth.Year;

                    if (referenceDate.DayOfYear < dateOfBirth.DayOfYear)
                    {
                        --age;
                    }

                    return Math.Max(0, age);
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error calculating age from date string: {DateString}",
                    dateOfBirthString
                );
                return 0;
            }
        }
    }
}
