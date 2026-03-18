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
        private readonly Dictionary<UserState, string> _statePrompts = new();

        public OpenAiAssistantService(ChatClient chatClient)
        {
            _chatClient = chatClient;
            LoadPrompts();
        }

        private void LoadPrompts()
        {
            var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts", "Assistant");
            var basePromptPath = Path.Combine(basePath, "Prompt_Base.txt");

            string basePrompt = File.Exists(basePromptPath)
                ? File.ReadAllText(basePromptPath)
                : "You are a helpful car insurance assistant. Always reply in the user's language.";

            foreach (UserState state in Enum.GetValues<UserState>())
            {
                var filePath = Path.Combine(basePath, $"Prompt_{state}.txt");

                if (File.Exists(filePath))
                {
                    var statePrompt = File.ReadAllText(filePath);
                    _statePrompts[state] = $"{basePrompt}\n\n=== CURRENT STATE INSTRUCTIONS ===\n{statePrompt}";
                }
                else
                {
                    _statePrompts[state] = $"{basePrompt}\n\nOUTPUT FORMAT: JSON ONLY {{\"replyText\": \"I need more information.\", \"nextState\": \"New\"}}";
                    Console.WriteLine($"[WARNING] Prompt file NOT FOUND for state: {state}");
                }
            }
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

                return ParseAiResponse(responseJson, session.State);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OpenAI Error]: {ex.Message}");
                return ("Sorry, the service is temporarily unavailable. Please try again later.", session.State);
            }
        }

        private string BuildSystemPrompt(UserSession session)
        {
            if (!_statePrompts.TryGetValue(session.State, out var template))
            {
                template = _statePrompts[UserState.New];
            }

            var validUntilDate = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-dd");

            var passportInfo = session.Passport != null
                ? $"First Name: {session.Passport.FirstName}, Last Name: {session.Passport.LastName}, Passport No: {session.Passport.DocumentNumber}"
                : "Not Provided";

            var vehicleInfo = session.Vehicle != null
                ? $"Vehicle Doc: {session.Vehicle.DocumentNumber}, VIN: {session.Vehicle.VinCode}, Make: {session.Vehicle.Make}, Model: {session.Vehicle.Model}"
                : "Not Provided";

            var userData = $"Passport Data: [{passportInfo}] | Vehicle Data: [{vehicleInfo}] | Valid Until: [{validUntilDate}]";

            return template.Replace("{0}", userData);
        }

        private (string ReplyText, UserState NextState) ParseAiResponse(string responseJson, UserState currentState)
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

            Console.WriteLine($"[AI DEBUG] Raw JSON NextState: '{rawNextState}' | Parsed Enum: {nextState}");

            return (aiResponse?.ReplyText ?? "Sorry, an internal error occurred.", nextState);
        }
    }
}
