namespace CarInsuranceBot.API.Application.Interfaces
{
    public interface IDocumentRecognitionService
    {
        Task<string> ExtractDataFromPhotoAsync(byte[] photoBytes);
    }
}
