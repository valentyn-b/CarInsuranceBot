namespace CarInsuranceBot.API.Application.Interfaces
{
    public interface IMessageService
    {
        Task SendMessageAsync(long chatId, string text);

        Task<byte[]> DownloadFileAsync(string fileId);
    }
}
