using CarInsuranceBot.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace CarInsuranceBot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IMessageHandler _messageHandler;

        public BotController(IMessageHandler messageHandler)
        {
            _messageHandler = messageHandler;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Bot is running and waiting for Telegram webhooks...");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            await _messageHandler.HandleUpdateAsync(update);
            return Ok();
        }
    }
}
