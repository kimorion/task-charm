using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Charm.Core.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<TelegramController> _logger;
        private readonly UserService _userService;

        public TelegramController(
            ITelegramBotClient client,
            CharmInterpreter interpreter,
            CharmManager manager,
            ILogger<TelegramController> logger,
            UserService userService)
        {
            _client = client;
            _interpreter = interpreter;
            _manager = manager;
            _logger = logger;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Update([Required] [FromBody] Update update)
        {
            await _userService.Initialize(update.Message.From);

            string? response = null;
            if (response is null && update.Type != UpdateType.Message)
            {
                response = $"Данный тип сообщений не поддерживается: {update.Type}";
            }

            if (response is null && update.Message.Type != MessageType.Text)
            {
                response = $"Данный тип сообщений не поддерживается: {update.Message.Type}";
            }

            if (response is null)
            {
                response = await _interpreter.TakeMessage(update.Message);
            }

            _logger.LogDebug(response);
            await _client.SendTextMessageAsync(update.Message.Chat.Id, response);
            return Ok();
        }
    }
}