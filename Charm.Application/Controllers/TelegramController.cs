using System;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Charm.Application.Controllers
{
    [ApiController]
    [Route("telegram-bot")]
    public class TelegramController : Controller
    {
        private readonly ITelegramBotClient _client;

        public TelegramController(ITelegramBotClient client)
        {
            _client = client;
        }

        [HttpPost]
        public async void Update([FromBody] Update update)
        {
            if (update.Type != UpdateType.Message)
            {
                Console.WriteLine($"Unsupported update type: {update.Type}");
                return;
            }

            var message = update.Message;
            if (message.Type != MessageType.Text)
            {
                await _client.SendTextMessageAsync(message.Chat.Id, "Данный тип сообщений не поддерживается!");
                return;
            }

            await _client.SendTextMessageAsync(message.Chat.Id, "Получен текст!");
        }

        [HttpGet]
        public IActionResult TestGet()
        {
            return Ok("Ok!");
        }
    }
}