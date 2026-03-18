using CarInsuranceBot.API.Application.DTOs;
using CarInsuranceBot.API.Application.Interfaces;

namespace CarInsuranceBot.API.Infrastructure.Services
{
    public class MockDocumentRecognitionService : IDocumentRecognitionService
    {
        public async Task<PassportRecognitionResultDto?> ExtractPassportDataAsync(byte[] fileBytes)
        {
            return new PassportRecognitionResultDto
            {
                // Fake data
                FirstName = "MockFirstName",
                LastName = "MockLastName",
                DocumentNumber = "PP123456",
                DateOfBirth = new DateTime(1990, 1, 1),
                IssueDate = new DateTime(2020, 5, 15),
                ExpiryDate = new DateTime(2030, 5, 15)
            };
        }

        public async Task<VehicleRecognitionResultDto?> ExtractVehicleDataAsync(byte[] fileBytes)
        {
            return new VehicleRecognitionResultDto
            {
                // Fake data
                VinCode = "JTD1234567890ABCD",
                LicensePlate = "KA7777AA",
                Make = "Toyota",
                Model = "Camry",
                YearOfManufacture = 2021,
                DocumentNumber = "CX987654",
                OwnerFullName = "MockFirstName MockLastName"
            };
        }
    }
}
