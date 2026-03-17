using CarInsuranceBot.API.Application.Interfaces;
using CarInsuranceBot.API.Infrastructure.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CarInsuranceBot.API.Infrastructure.Services
{
    public class MessageHandler : IMessageHandler
    {
        private readonly IUserStateStorage _stateStorage;
        private readonly IMessageService _messageService;
        private readonly IAiAssistantService _aiAssistantService;
        private readonly IDocumentRecognitionService _documentRecognitionService;

        public MessageHandler(IUserStateStorage stateStorage, IMessageService messageService, IAiAssistantService aiAssistantService, IDocumentRecognitionService documentRecognitionService)
        {
            _stateStorage = stateStorage;
            _messageService = messageService;
            _aiAssistantService = aiAssistantService;
            _documentRecognitionService = documentRecognitionService;            
        }

        public async Task HandleUpdateAsync(Update update)
        {
            if (update.Type == UpdateType.Message && update.Message != null)
            {
                var chatId = update.Message.Chat.Id;
                var userName = update.Message.From?.FirstName ?? "Unknown";
                string messageText = string.Empty;

                if (update.Message.Type == MessageType.Text)
                {
                    messageText = update.Message.Text!;
                    Console.WriteLine($"[MessageHandler] Received text '{messageText}' from {userName}");
                }

                else if (update.Message.Type == MessageType.Photo)
                {
                    var photo = update.Message.Photo!.Last();

                    var file = await _messageService.DownloadFileAsync(photo.FileId);

                    Console.WriteLine("[MessageHandler] Extracting data from photo...");

                    var extractedData = await _documentRecognitionService.ExtractDataFromPhotoAsync(file);

                    messageText = $"[PHOTO_RECEIVED: {extractedData}]";
                    Console.WriteLine($"[MessageHandler] Data extracted successfully.");
                }

                else
                {
                    return;
                }

                var currentState = _stateStorage.GetUserState(chatId);
                var (replyText, newState) = await _aiAssistantService.ProcessUserMessageAsync(messageText, currentState);
                _stateStorage.SetUserState(chatId, newState);

                await _messageService.SendMessageAsync(chatId, replyText);
            }
        }
    }
}
