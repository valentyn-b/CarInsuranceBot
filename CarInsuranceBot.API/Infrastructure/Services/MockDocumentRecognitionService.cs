using CarInsuranceBot.API.Application.Interfaces;

namespace CarInsuranceBot.API.Infrastructure.Services
{
    public class MockDocumentRecognitionService : IDocumentRecognitionService
    {
        public async Task<string> ExtractDataFromPhotoAsync(byte[] photoBytes)
        {
            // Recognition imitation
            await Task.Delay(2000);

            return "First Name: John, Last Name: Smith, Document Number: AB123456, Car: VW Golf";
        }
    }
}
