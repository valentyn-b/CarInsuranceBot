using CarInsuranceBot.API.Application.Enums;
using CarInsuranceBot.API.Application.Interfaces;

namespace CarInsuranceBot.API.Infrastructure.Services
{
    public class MockAiAssistantService : IAiAssistantService
    {
        public async Task<(string ReplyText, UserState NextState)> ProcessUserMessageAsync(string message, UserState currentState)
        {
            var text = message.ToLower();

            return currentState switch
            {
                UserState.New or UserState.Finished => (
                    "[AI Assistant Response: Greeting the user and asking to submit a photo of their passport and vehicle identification document]",
                    UserState.WaitingForDocuments
                ),

                UserState.WaitingForDocuments => (
                    "[AI Assistant Response: Simulating Mindee API data extraction. Displaying mock data (e.g., Name: ***, Car: ***) and asking the user for confirmation]",
                    UserState.ConfirmingData
                ),

                UserState.ConfirmingData => text.Contains("yes")
                    ? ("[AI Assistant Response: Acknowledging data confirmation. Informing that the fixed price is 100 USD and asking for agreement]", UserState.AwaitingPriceAgreement)
                    : ("[AI Assistant Response: Acknowledging data rejection. Apologizing and asking the user to retake and resubmit the photos]", UserState.WaitingForDocuments),

                UserState.AwaitingPriceAgreement => text.Contains("yes")
                    ? ("[AI Assistant Response: Acknowledging price agreement. Generating and sending the dummy insurance policy document]", UserState.Finished)
                    : ("[AI Assistant Response: Handling price disagreement. Apologizing and explaining that 100 USD is the only available price]", UserState.Finished),

                _ => (
                    "[AI Assistant Response: Fallback message for unrecognized input. Suggesting to type /start to begin again]",
                    UserState.New
                )
            };
        }
    }
}
