using CarInsuranceBot.API.Application.DTOs;
using CarInsuranceBot.API.Application.Interfaces;
using OpenAI.Chat;
using System.Text.Json;

namespace CarInsuranceBot.API.Infrastructure.Services
{
    public class OpenAiDocumentRecognitionService : IDocumentRecognitionService
    {
        private readonly ChatClient _chatClient;
        private readonly IPromptProvider _promptProvider;

        public OpenAiDocumentRecognitionService(ChatClient chatClient, IPromptProvider promptProvider)
        {
            _chatClient = chatClient;
            _promptProvider = promptProvider;
        }

        // TODO: In a real production environment handling sensitive PII (passports), 
        // ensure compliance with GDPR. Consider using dedicated on-premise OCR 
        // or Azure AI with strict Data Processing Agreements (DPA).
        public async Task<PassportRecognitionResultDto?> ExtractPassportDataAsync(byte[] fileBytes)
        {
            try
            {
                return await ExtractDataAsync<PassportRecognitionResultDto>(_promptProvider.GetPassportRecognitionPrompt(), fileBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OpenAI Vision Error - Passport]: {ex.Message}");
                return null;
            }
        }

        public async Task<VehicleRecognitionResultDto?> ExtractVehicleDataAsync(byte[] fileBytes)
        {
            try
            {
                return await ExtractDataAsync<VehicleRecognitionResultDto>(_promptProvider.GetVehicleRecognitionPrompt(), fileBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OpenAI Vision Error - Vehicle]: {ex.Message}");
                return null;
            }
        }

        private async Task<T?> ExtractDataAsync<T>(string prompt, byte[] fileBytes) where T : class
        {
            BinaryData imageBytes = BinaryData.FromBytes(fileBytes);

            List<ChatMessage> messages = new()
            {
                new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart(prompt),
                    ChatMessageContentPart.CreateImagePart(imageBytes, "image/jpeg")
                )
            };

            var options = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
                Temperature = 0.0f
            };

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, options);
            var responseJson = completion.Content[0].Text;

            Console.WriteLine($"[OpenAI Vision Raw JSON]: {responseJson}");

            return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
