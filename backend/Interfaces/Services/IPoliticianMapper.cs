using backend.DTO;
using backend.Models;
using System; // For DateOnly
using System.Collections.Generic;


namespace backend.Interfaces.Services
{
    public interface IPoliticianMapper
    {
        DailyPoliticianDto MapToDetailsDto(Aktor aktor, DateOnly referenceDate);
        List<SearchListDto> MapToSummaryDtoList(IEnumerable<Aktor> aktors);
        SearchListDto MapToSummaryDto(Aktor aktor);
    }
}