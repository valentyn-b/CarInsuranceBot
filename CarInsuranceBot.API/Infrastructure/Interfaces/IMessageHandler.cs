using Telegram.Bot.Types;

namespace CarInsuranceBot.API.Infrastructure.Interfaces
{
    public interface IMessageHandler
    {
        Task HandleUpdateAsync(Update update);
    }
}
