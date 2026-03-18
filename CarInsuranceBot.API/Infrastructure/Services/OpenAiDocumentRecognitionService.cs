using CarInsuranceBot.API.Application.DTOs;
using CarInsuranceBot.API.Application.Interfaces;
using OpenAI.Chat;
using System.Text.Json;

namespace CarInsuranceBot.API.Infrastructure.Services
{
    public class OpenAiDocumentRecognitionService : IDocumentRecognitionService
    {
        private readonly ChatClient _chatClient;
        private readonly string _passportPromptPath;
        private readonly string _vehiclePromptPath;

        public OpenAiDocumentRecognitionService(ChatClient chatClient)
        {
            _chatClient = chatClient;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _passportPromptPath = Path.Combine(baseDir, "Prompts", "Recognition", "Prompt_Passport.txt");
            _vehiclePromptPath = Path.Combine(baseDir, "Prompts", "Recognition", "Prompt_Vehicle.txt");
        }

        // TODO: In a real production environment handling sensitive PII (passports), 
        // ensure compliance with GDPR. Consider using dedicated on-premise OCR 
        // or Azure AI with strict Data Processing Agreements (DPA).
        public async Task<PassportRecognitionResultDto?> ExtractPassportDataAsync(byte[] fileBytes)
        {
            try
            {
                if (!File.Exists(_passportPromptPath))
                    throw new FileNotFoundException($"Prompt file not found at {_passportPromptPath}");

                string prompt = await File.ReadAllTextAsync(_passportPromptPath);
                return await ExtractDataAsync<PassportRecognitionResultDto>(prompt, fileBytes);
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
                if (!File.Exists(_vehiclePromptPath))
                    throw new FileNotFoundException($"Prompt file not found at {_vehiclePromptPath}");

                string prompt = await File.ReadAllTextAsync(_vehiclePromptPath);
                return await ExtractDataAsync<VehicleRecognitionResultDto>(prompt, fileBytes);
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
