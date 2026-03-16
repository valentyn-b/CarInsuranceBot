namespace CarInsuranceBot.API.Application.Interfaces
{
    public interface IMessageSender
    {
        Task SendMessageAsync(long chatId, string text);
    }
}
