using backend.DTO;
using backend.Models;
using System; // For DateOnly
using System.Collections.Generic;


namespace backend.Interfaces.Services
{
    public interface IPoliticianMapper
    {
        PoliticianDetailsDto MapToDetailsDto(Aktor aktor, DateOnly referenceDate);
        List<PoliticianSummaryDto> MapToSummaryDtoList(IEnumerable<Aktor> aktors);
        PoliticianSummaryDto MapToSummaryDto(Aktor aktor);
    }
}