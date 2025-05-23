using System; // For DateOnly
using System.Collections.Generic;
using backend.DTO;
using backend.Models.Politicians;

namespace backend.Interfaces.Services
{
    public interface IPoliticianMapper
    {
        DailyPoliticianDto MapToDetailsDto(Aktor aktor, DateOnly referenceDate);
        List<SearchListDto> MapToSummaryDtoList(IEnumerable<Aktor> aktors);
        SearchListDto MapToSummaryDto(Aktor aktor);
        DailyPoliticianDto MapToDetailsDto(Aktor aktor);
    }
}
