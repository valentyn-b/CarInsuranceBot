using CarInsuranceBot.API.Application.DTOs;

namespace CarInsuranceBot.API.Application.Interfaces
{
    public interface IDocumentRecognitionService
    {
        Task<PassportRecognitionResultDto?> ExtractPassportDataAsync(byte[] fileBytes);

        Task<VehicleRecognitionResultDto?> ExtractVehicleDataAsync(byte[] fileBytes);
    }
}
