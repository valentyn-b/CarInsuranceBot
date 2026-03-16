using Telegram.Bot.Types;

namespace CarInsuranceBot.API.Application.Interfaces
{
    public interface IMessageHandler
    {
        Task HandleUpdateAsync(Update update);
    }
}
