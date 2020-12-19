using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Charm.Core.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly CharmInterpreterService _interpreterService;
        private readonly CharmManager _manager;
        private readonly ILogger<TelegramController> _logger;
        private readonly UserService _userService;
        private readonly string _adminId;

        public TelegramController(
            ITelegramBotClient client,
            CharmInterpreterService interpreterService,
            CharmManager manager,
            ILogger<TelegramController> logger,
            UserService userService,
            IConfiguration configuration)
        {
            _client = client;
            _interpreterService = interpreterService;
            _manager = manager;
            _logger = logger;
            _userService = userService;
            _adminId = configuration["TelegramAdminId"];
        }

        [HttpPost]
        public async Task<IActionResult> Update([Required] [FromBody] Update update)
        {
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
                try
                {
                    await _userService.Initialize(update.Message.From);
                    response = await _interpreterService.TakeMessage(update.Message);
                }
                catch (Exception e)
                {
                    await _client.SendTextMessageAsync(int.Parse(_adminId),
                        "Произошла ошибка приложения: \n" + e.Message, ParseMode.Html);
                }
            }

            _logger.LogDebug(response);

            if (update.Message != null)
                await _client.SendTextMessageAsync(update.Message.From.Id, response, ParseMode.Html);
            return Ok();
        }
    }
}