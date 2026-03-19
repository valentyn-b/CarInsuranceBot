using CarInsuranceBot.API.Application.Enums;

namespace CarInsuranceBot.API.Application.Interfaces
{
    public interface IPromptProvider
    {
        string GetAssistantPrompt(UserState state);
        string GetPassportRecognitionPrompt();
        string GetVehicleRecognitionPrompt();
    }
}
