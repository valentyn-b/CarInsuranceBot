using CarInsuranceBot.API.Application.DTOs;
using CarInsuranceBot.API.Application.Interfaces;
using OpenAI.Chat;
using System.Text.Json;

namespace CarInsuranceBot.API.Infrastructure.Services
{
    public class OpenAiDocumentRecognitionService : IDocumentRecognitionService
    {
        private readonly ChatClient _chatClient;
        private readonly string _passportPrompt;
        private readonly string _vehiclePrompt;

        public OpenAiDocumentRecognitionService(ChatClient chatClient)
        {
            _chatClient = chatClient;

            LoadPrompts(out _passportPrompt, out _vehiclePrompt);
        }

        private void LoadPrompts(out string passportPrompt, out string vehiclePrompt)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string promptsPath = Path.Combine(baseDir, "Prompts", "Recognition");

            string basePromptPath = Path.Combine(promptsPath, "Prompt_Base.txt");
            string passportPath = Path.Combine(promptsPath, "Prompt_Passport.txt");
            string vehiclePath = Path.Combine(promptsPath, "Prompt_Vehicle.txt");

            string basePrompt = File.Exists(basePromptPath)
                ? File.ReadAllText(basePromptPath)
                : "Extract data to JSON.";

            string passportSpecific = File.Exists(passportPath) ? File.ReadAllText(passportPath) : "";
            string vehicleSpecific = File.Exists(vehiclePath) ? File.ReadAllText(vehiclePath) : "";

            passportPrompt = $"{basePrompt}\n\n=== SPECIFIC INSTRUCTIONS ===\n{passportSpecific}";
            vehiclePrompt = $"{basePrompt}\n\n=== SPECIFIC INSTRUCTIONS ===\n{vehicleSpecific}";
        }

        // TODO: In a real production environment handling sensitive PII (passports), 
        // ensure compliance with GDPR. Consider using dedicated on-premise OCR 
        // or Azure AI with strict Data Processing Agreements (DPA).
        public async Task<PassportRecognitionResultDto?> ExtractPassportDataAsync(byte[] fileBytes)
        {
            try
            {
                return await ExtractDataAsync<PassportRecognitionResultDto>(_passportPrompt, fileBytes);
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
                return await ExtractDataAsync<VehicleRecognitionResultDto>(_vehiclePrompt, fileBytes);
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
