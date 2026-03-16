using CarInsuranceBot.API.Application.Interfaces;
using Telegram.Bot;

namespace CarInsuranceBot.API.Infrastructure.Services
{
    public class MessageSender : IMessageSender
    {
        private readonly ITelegramBotClient _botClient;

        public MessageSender(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task SendMessageAsync(long chatId, string text)
        {
            await _botClient.SendMessage(chatId, text);
        }
    }
}
