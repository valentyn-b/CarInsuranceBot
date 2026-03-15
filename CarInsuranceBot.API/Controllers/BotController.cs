using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CarInsuranceBot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly ITelegramBotClient _botClient;

        public BotController(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Bot is running and waiting for Telegram webhooks...");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;
                var userName = update.Message.From?.FirstName ?? "Незнайомець";

                Console.WriteLine($"Received message '{messageText}' from {userName}");

                var replyText = $"Привіт, {userName}! Я отримав твоє повідомлення: \"{messageText}\". Мій двигун працює!";

                await _botClient.SendMessage(chatId: chatId, text: replyText);
            }

            return Ok();
        }
    }
}
