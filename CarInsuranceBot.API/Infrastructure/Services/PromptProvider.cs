using CarInsuranceBot.API.Application.Enums;
using CarInsuranceBot.API.Application.Interfaces;

namespace CarInsuranceBot.API.Infrastructure.Services
{
    public class PromptProvider : IPromptProvider
    {
        private readonly Dictionary<UserState, string> _assistantPrompts = new();
        private readonly string _passportPrompt;
        private readonly string _vehiclePrompt;

        public PromptProvider()
        {
            var assistantBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts", "Assistant");
            var baseAssistantPromptPath = Path.Combine(assistantBasePath, "Prompt_Base.txt");

            string baseAssistantPrompt = File.Exists(baseAssistantPromptPath)
                ? File.ReadAllText(baseAssistantPromptPath)
                : "You are a helpful car insurance assistant.";

            foreach (UserState state in Enum.GetValues<UserState>())
            {
                var filePath = Path.Combine(assistantBasePath, $"Prompt_{state}.txt");
                if (File.Exists(filePath))
                {
                    var statePrompt = File.ReadAllText(filePath);
                    _assistantPrompts[state] = $"{baseAssistantPrompt}\n\n=== CURRENT STATE INSTRUCTIONS ===\n{statePrompt}";
                }
                else
                {
                    _assistantPrompts[state] = $"{baseAssistantPrompt}\n\nOUTPUT FORMAT: JSON ONLY {{\"replyText\": \"I need more information.\", \"nextState\": \"New\"}}";
                }
            }

            var recognitionBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts", "Recognition");
            var baseRecognitionPromptPath = Path.Combine(recognitionBasePath, "Prompt_Base.txt");

            string baseRecognitionPrompt = File.Exists(baseRecognitionPromptPath)
                ? File.ReadAllText(baseRecognitionPromptPath)
                : "Extract data to JSON.";

            var passportPath = Path.Combine(recognitionBasePath, "Prompt_Passport.txt");
            var vehiclePath = Path.Combine(recognitionBasePath, "Prompt_Vehicle.txt");

            string passportSpecific = File.Exists(passportPath) ? File.ReadAllText(passportPath) : "";
            string vehicleSpecific = File.Exists(vehiclePath) ? File.ReadAllText(vehiclePath) : "";

            _passportPrompt = $"{baseRecognitionPrompt}\n\n=== SPECIFIC INSTRUCTIONS ===\n{passportSpecific}";
            _vehiclePrompt = $"{baseRecognitionPrompt}\n\n=== SPECIFIC INSTRUCTIONS ===\n{vehicleSpecific}";
        }

        public string GetAssistantPrompt(UserState state) =>
            _assistantPrompts.TryGetValue(state, out var prompt) ? prompt : _assistantPrompts[UserState.New];

        public string GetPassportRecognitionPrompt() => _passportPrompt;

        public string GetVehicleRecognitionPrompt() => _vehiclePrompt;
    }
}
