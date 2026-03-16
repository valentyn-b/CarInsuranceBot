namespace CarInsuranceBot.API.Infrastructure.DTOs
{
    internal class AiResponseDto
    {
        public string ReplyText { get; set; } = string.Empty;
        public string NextState { get; set; } = string.Empty;
    }
}
