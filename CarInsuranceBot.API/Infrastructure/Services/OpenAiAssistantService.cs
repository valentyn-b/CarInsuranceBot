using CarInsuranceBot.API.Application.Enums;
using CarInsuranceBot.API.Application.Interfaces;
using CarInsuranceBot.API.Infrastructure.DTOs;
using OpenAI.Chat;
using System.Text.Json;

namespace CarInsuranceBot.API.Infrastructure.Services
{
    public class OpenAiAssistantService : IAiAssistantService
    {
        private readonly ChatClient _chatClient;
        private readonly string _systemPromptTemplate;

        public OpenAiAssistantService(ChatClient chatClient)
        {
            _chatClient = chatClient;

            var promptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts", "InsuranceSystemPrompt.txt");
            _systemPromptTemplate = File.ReadAllText(promptPath);
        }

        public async Task<(string ReplyText, UserState NextState)> ProcessUserMessageAsync(string message, UserState currentState)
        {
            var systemPrompt = string.Format(_systemPromptTemplate, currentState.ToString());

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

                var aiResponse = JsonSerializer.Deserialize<AiResponseDto>(
                    responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                var nextState = Enum.TryParse<UserState>(aiResponse?.NextState, out var parsedState)
                    ? parsedState
                    : currentState;

                return (aiResponse?.ReplyText ?? "Sorry, an internal error occurred.", nextState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OpenAI Error]: {ex.Message}");
                return ("Sorry, the service is temporarily unavailable. Please try again later.", currentState);
            }
        }
    }
}
