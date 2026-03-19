using CarInsuranceBot.API.Application.Enums;
using CarInsuranceBot.API.Application.Interfaces;
using CarInsuranceBot.API.Core.Entities;
using CarInsuranceBot.API.Infrastructure.DTOs;
using OpenAI.Chat;
using System.Text.Json;

namespace CarInsuranceBot.API.Infrastructure.Services
{
    public class OpenAiAssistantService : IAiAssistantService
    {
        private readonly ChatClient _chatClient;
        private readonly IPromptProvider _promptProvider;

        public OpenAiAssistantService(ChatClient chatClient, IPromptProvider promptProvider)
        {
            _chatClient = chatClient;
            _promptProvider = promptProvider;
        }              

        public async Task<(string ReplyText, UserState NextState)> ProcessUserMessageAsync(string message, UserSession session)
        {
            var systemPrompt = BuildSystemPrompt(session);

            List<ChatMessage> messages = new()
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(message)
            };

            var options = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            try
            {
                ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, options);
                var responseJson = completion.Content[0].Text;

                var (replyText, nextState, detectedLanguage) = ParseAiResponse(responseJson, session.State);

                if (!string.IsNullOrWhiteSpace(detectedLanguage) && detectedLanguage != "Unknown")
                {
                    session.Language = detectedLanguage;
                }

                return (replyText, nextState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OpenAI Error]: {ex.Message}");
                return ("Sorry, the service is temporarily unavailable. Please try again later.", session.State);
            }
        }

        private string BuildSystemPrompt(UserSession session)
        {
            var template = _promptProvider.GetAssistantPrompt(session.State);

            string preferredName = session.Passport?.FirstName ?? session.TelegramName;

            var validUntilDate = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-dd");

            var passportInfo = session.Passport != null
                ? $"First Name: {session.Passport.FirstName}, Last Name: {session.Passport.LastName}, Passport No: {session.Passport.DocumentNumber}"
                : "Not Provided";

            var vehicleInfo = session.Vehicle != null
                ? $"Vehicle Doc: {session.Vehicle.DocumentNumber}, VIN: {session.Vehicle.VinCode}, Make: {session.Vehicle.Make}, Model: {session.Vehicle.Model}"
                : "Not Provided";

            var userData = $"Passport Data: [{passportInfo}] | Vehicle Data: [{vehicleInfo}] | Valid Until: [{validUntilDate}]";

            return template
                .Replace("{0}", userData)
                .Replace("{1}", session.Language)
                .Replace("{2}", preferredName);
        }

        private (string ReplyText, UserState NextState, string? DetectedLanguage) ParseAiResponse(string responseJson, UserState currentState)
        {
            var aiResponse = JsonSerializer.Deserialize<AiResponseDto>(
                responseJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            string rawNextState = aiResponse?.NextState ?? string.Empty;

            string cleanNextState = rawNextState
                .Replace(".", "")
                .Replace("\"", "")
                .Replace(" ", "")
                .Trim();

            var nextState = Enum.TryParse<UserState>(cleanNextState, true, out var parsedState)
                ? parsedState
                : currentState;

            Console.WriteLine($"[AI DEBUG] Raw JSON NextState: '{rawNextState}' | Parsed Enum: {nextState} | Language: {aiResponse?.DetectedLanguage}");

            return (aiResponse?.ReplyText ?? "Sorry, an internal error occurred.", nextState, aiResponse?.DetectedLanguage);
        }
    }
}
