namespace CarInsuranceBot.API.Application.DTOs
{
    public class VehicleRecognitionResultDto
    {
        public bool IsValidDocument { get; set; }
        public string? ImageDescription { get; set; }
        public string? VinCode { get; set; }
        public string? LicensePlate { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? YearOfManufacture { get; set; }
        public string? DocumentNumber { get; set; }
        public string? OwnerFullName { get; set; }
    }
}
