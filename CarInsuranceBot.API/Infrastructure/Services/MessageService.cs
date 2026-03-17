using CarInsuranceBot.API.Application.Interfaces;
using Telegram.Bot;

namespace CarInsuranceBot.API.Infrastructure.Services
{
    public class MessageService : IMessageService
    {
        private readonly ITelegramBotClient _botClient;

        public MessageService(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task SendMessageAsync(long chatId, string text)
        {
            await _botClient.SendMessage(chatId, text);
        }

        public async Task<byte[]> DownloadFileAsync(string fileId)
        {
            var file = await _botClient.GetFile(fileId);

            using var memoryStream = new MemoryStream();
            await _botClient.DownloadFile(file.FilePath!, memoryStream);

            return memoryStream.ToArray();
        }
    }
}
