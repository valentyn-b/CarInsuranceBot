namespace CarInsuranceBot.API.Core.Entities
{
    public class VehicleData
    {
        public string VinCode { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int? YearOfManufacture { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string OwnerFullName { get; set; } = string.Empty;
    }
}
