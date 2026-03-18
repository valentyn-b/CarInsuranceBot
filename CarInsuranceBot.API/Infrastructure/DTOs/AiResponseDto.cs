namespace CarInsuranceBot.API.Infrastructure.DTOs
{
    internal class AiResponseDto
    {
        public string? ReplyText { get; set; }
        public string? NextState { get; set; }
        public string? DetectedLanguage { get; set; }
    }
}
