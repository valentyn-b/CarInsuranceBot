namespace CarInsuranceBot.API.Application.DTOs
{
    public class PassportRecognitionResultDto
    {
        public bool IsValidDocument { get; set; }
        public string? ImageDescription { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DocumentNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
