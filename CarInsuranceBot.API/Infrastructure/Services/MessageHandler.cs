using CarInsuranceBot.API.Application.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CarInsuranceBot.API.Infrastructure.Services
{
    public class MessageHandler : IMessageHandler
    {
        private readonly IMessageSender _messageSender;
        private readonly IAiAssistantService _aiAssistantService;
        private readonly IUserStateStorage _stateStorage;

        public MessageHandler(IMessageSender messageSender, IAiAssistantService aiAssistantService, IUserStateStorage stateStorage)
        {
            _messageSender = messageSender;
            _aiAssistantService = aiAssistantService;
            _stateStorage = stateStorage;
        }

        public async Task HandleUpdateAsync(Update update)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;
                var userName = update.Message.From?.FirstName ?? "Незнайомець";

                Console.WriteLine($"[MessageHandler] Отримано '{messageText}' від {userName}");

                var currentState = _stateStorage.GetUserState(chatId);
                var (replyText, newState) = await _aiAssistantService.ProcessUserMessageAsync(messageText, currentState);
                _stateStorage.SetUserState(chatId, newState);

                await _messageSender.SendMessageAsync(chatId, replyText);
            }
        }
    }
}
