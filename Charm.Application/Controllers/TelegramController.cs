using System;
using System.Threading.Tasks;
using Charm.Core.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Charm.Application.Controllers
{
    [ApiController]
    [Route("telegram-bot")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TelegramController : ControllerBase
    {
        private readonly ITelegramBotClient _client;
        private readonly CharmInterpreter _interpreter;
        private readonly CharmManager _manager;

        public TelegramController(ITelegramBotClient client, CharmInterpreter interpreter, CharmManager manager)
        {
            _client = client;
            _interpreter = interpreter;
            _manager = manager;
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] Update update)
        {

            if (update.Type != UpdateType.Message)
            {
                Console.WriteLine($"Unsupported update type: {update.Type}");
                return Ok();
            }

            var message = update.Message;
            if (message.Type != MessageType.Text)
            {
                await _client.SendTextMessageAsync(message.Chat.Id, "Данный тип сообщений не поддерживается!");
                return Ok();
            }

            await _manager.CreateUserIfNotExists(update.Message.Chat.Id, update.Message.Chat.Username);
            var response = await _interpreter.TakeTextMessage(message.Chat.Id, message.Text);

            await _client.SendTextMessageAsync(message.Chat.Id, response);
            return Ok();
        }
    }
}