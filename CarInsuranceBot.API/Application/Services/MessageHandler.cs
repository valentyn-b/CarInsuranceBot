using CarInsuranceBot.API.Application.Enums;
using CarInsuranceBot.API.Application.Interfaces;
using CarInsuranceBot.API.Core.Entities;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CarInsuranceBot.API.Application.Services
{
    public class MessageHandler : IMessageHandler
    {
        private readonly IUserSessionStorage _sessionStorage;
        private readonly IMessageService _messageService;
        private readonly IAiAssistantService _aiAssistantService;
        private readonly IDocumentRecognitionService _documentRecognitionService;
        private readonly IMemoryCache _memoryCache;

        public MessageHandler(
            IUserSessionStorage sessionStorage,
            IMessageService messageService,
            IAiAssistantService aiAssistantService,
            IDocumentRecognitionService documentRecognitionService,
            IMemoryCache memoryCache)
        {
            _sessionStorage = sessionStorage;
            _messageService = messageService;
            _aiAssistantService = aiAssistantService;
            _documentRecognitionService = documentRecognitionService;
            _memoryCache = memoryCache;
        }

        public async Task HandleUpdateAsync(Update update)
        {
            if (update.Type != UpdateType.Message || update.Message == null)
                return;

            if (update.Message.MediaGroupId != null)
            {
                if (_memoryCache.TryGetValue(update.Message.MediaGroupId, out _))
                {
                    return;
                }
                _memoryCache.Set(update.Message.MediaGroupId, true, TimeSpan.FromMinutes(2));
            }

            var chatId = update.Message.Chat.Id;
            var session = _sessionStorage.GetSession(chatId);

            if (update.Message.From != null && !string.IsNullOrEmpty(update.Message.From.FirstName))
            {
                session.TelegramName = update.Message.From.FirstName;
            }

            string messageContentForAi = await ProcessMessageContentAsync(update.Message, session);

            if (string.IsNullOrEmpty(messageContentForAi))
                return;

            var (replyText, newState) = await _aiAssistantService.ProcessUserMessageAsync(messageContentForAi, session);

            newState = ApplyGuardrails(session.State, newState, messageContentForAi);

            session.State = newState;
            _sessionStorage.SaveSession(chatId, session);

            await _messageService.SendMessageAsync(chatId, replyText);

            Console.WriteLine($"[MessageHandler] Chat: {chatId} | State Transition: {session.State} -> {newState}");
        }

        private async Task<string> ProcessMessageContentAsync(Message message, UserSession session)
        {
            if (message.Type == MessageType.Text)
            {
                return HandleTextAsync(message, session);
            }
            else if (message.Type == MessageType.Photo)
            {
                return await HandlePhotoAsync(message, session);
            }

            return string.Empty;
        }

        private string HandleTextAsync(Message message, UserSession session)
        {
            var text = message.Text!;
            var userName = message.From?.FirstName ?? "Unknown";
            Console.WriteLine($"[MessageHandler] Received text '{text}' from {userName}");

            if (text.Trim().Equals("/start", StringComparison.OrdinalIgnoreCase))
            {
                session.State = UserState.New;
                session.Passport = null;
                session.Vehicle = null;
                Console.WriteLine($"[MessageHandler] Hard reset for user {message.Chat.Id} via /start.");
            }

            return text;
        }

        private async Task<string> HandlePhotoAsync(Message message, UserSession session)
        {
            if (session.State != UserState.WaitingForPassport && session.State != UserState.WaitingForVehicleDoc)
            {
                Console.WriteLine($"[MessageHandler] Ignored photo received in state: {session.State}");
                return "[SYSTEM_INFO: The user sent a photo, but NO PHOTO IS EXPECTED at this stage. Politely tell them that you don't need any photos right now. If you are waiting for a Yes/No confirmation, strictly ask them to answer Yes or No.]";
            }

            var photo = message.Photo!.Last();
            var fileBytes = await _messageService.DownloadFileAsync(photo.FileId);

            Console.WriteLine("[MessageHandler] Extracting data from photo...");

            if (session.State == UserState.WaitingForPassport)
            {
                var extractedData = await _documentRecognitionService.ExtractPassportDataAsync(fileBytes);
                if (extractedData != null)
                {
                    if (!extractedData.IsValidDocument)
                    {
                        Console.WriteLine($"[MessageHandler] Invalid passport image: {extractedData.ImageDescription}");
                        return $"[SYSTEM_INFO: The user uploaded an image, but it is NOT a valid passport/ID. It looks like: \"{extractedData.ImageDescription}\". Joke about it politely and ask to upload the actual document.]";
                    }

                    Console.WriteLine($"[MessageHandler] Passport extracted: {extractedData.FirstName} {extractedData.LastName}");

                    session.Passport = new PassportData
                    {
                        FirstName = extractedData.FirstName,
                        LastName = extractedData.LastName,
                        DocumentNumber = extractedData.DocumentNumber,
                        DateOfBirth = extractedData.DateOfBirth,
                        IssueDate = extractedData.IssueDate,
                        ExpiryDate = extractedData.ExpiryDate
                    };

                    var dob = session.Passport.DateOfBirth?.ToString("yyyy-MM-dd") ?? "Unknown";
                    var issue = session.Passport.IssueDate?.ToString("yyyy-MM-dd") ?? "Unknown";
                    var expiry = session.Passport.ExpiryDate?.ToString("yyyy-MM-dd") ?? "Unknown";

                    return $"[PHOTO_RECEIVED: First Name: {session.Passport.FirstName}, Last Name: {session.Passport.LastName}, Document Number: {session.Passport.DocumentNumber}, Date of Birth: {dob}, Issue Date: {issue}, Expiry Date: {expiry}]";
                }
            }
            else if (session.State == UserState.WaitingForVehicleDoc)
            {
                var extractedData = await _documentRecognitionService.ExtractVehicleDataAsync(fileBytes);
                if (extractedData != null)
                {
                    if (!extractedData.IsValidDocument)
                    {
                        Console.WriteLine($"[MessageHandler] Invalid vehicle doc image: {extractedData.ImageDescription}");
                        return $"[SYSTEM_INFO: The user uploaded an image, but it is NOT a valid vehicle document. It looks like: \"{extractedData.ImageDescription}\". Joke about it politely and ask to upload the actual document.]";
                    }

                    Console.WriteLine($"[MessageHandler] Vehicle Doc extracted: {extractedData.DocumentNumber}");

                    session.Vehicle = new VehicleData
                    {
                        DocumentNumber = extractedData.DocumentNumber,
                        VinCode = extractedData.VinCode,
                        Make = extractedData.Make,
                        Model = extractedData.Model,
                        YearOfManufacture = extractedData.YearOfManufacture,
                        LicensePlate = extractedData.LicensePlate,
                        OwnerFullName = extractedData.OwnerFullName
                    };

                    var year = session.Vehicle.YearOfManufacture?.ToString() ?? "Unknown";

                    return $"[PHOTO_RECEIVED: Document Number: {session.Vehicle.DocumentNumber}, VIN: {session.Vehicle.VinCode}, Make: {session.Vehicle.Make}, Model: {session.Vehicle.Model}, Year: {year}, License Plate: {session.Vehicle.LicensePlate}, Owner: {session.Vehicle.OwnerFullName}]";
                }
            }

            Console.WriteLine("[MessageHandler] Failed to extract data.");
            return "[PHOTO_ERROR: Could not read document. Please ask user to send a better photo.]";
        }

        private UserState ApplyGuardrails(UserState currentState, UserState suggestedState, string messageContent)
        {
            if (currentState == UserState.WaitingForPassport)
            {
                if (messageContent.Contains("[PHOTO_RECEIVED"))
                {
                    return UserState.ConfirmingPassport;
                }

                if (suggestedState != UserState.WaitingForPassport)
                {
                    Console.WriteLine("[SECURITY] AI tried to skip Passport upload. Access denied.");
                }

                return UserState.WaitingForPassport;
            }

            if (currentState == UserState.WaitingForVehicleDoc)
            {
                if (messageContent.Contains("[PHOTO_RECEIVED"))
                {
                    return UserState.ConfirmingVehicleDoc;
                }

                if (suggestedState != UserState.WaitingForVehicleDoc)
                {
                    Console.WriteLine("[SECURITY] AI tried to skip Vehicle Doc upload. Access denied.");
                }

                return UserState.WaitingForVehicleDoc;
            }

            return suggestedState;
        }
    }
}
