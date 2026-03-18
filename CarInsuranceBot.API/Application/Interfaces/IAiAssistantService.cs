using CarInsuranceBot.API.Application.Enums;
using CarInsuranceBot.API.Core.Entities;

namespace CarInsuranceBot.API.Application.Interfaces
{
    public interface IAiAssistantService
    {
        Task<(string ReplyText, UserState NextState)> ProcessUserMessageAsync(string message, UserSession session);
    }
}
