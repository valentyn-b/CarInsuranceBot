using CarInsuranceBot.API.Application.Enums;

namespace CarInsuranceBot.API.Application.Interfaces
{
    public interface IAiAssistantService
    {
        Task<(string ReplyText, UserState NextState)> ProcessUserMessageAsync(string message, UserState currentState);
    }
}
