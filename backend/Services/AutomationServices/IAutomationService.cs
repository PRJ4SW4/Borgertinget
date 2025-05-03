// /backend/Services/AutomationServices/IAutomationService.cs
namespace backend.Services.AutomationServices;

public interface IAutomationService
{
    Task<int> RunAutomation();
}
