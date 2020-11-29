﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Charm.Core.Domain.Entities;
using Charm.Core.Domain.SpeechCases;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Charm.Core.Domain.Services
{
    public class CharmInterpreter
    {
        private readonly ILogger<CharmInterpreter> _logger;
        private readonly CharmManager _manager;
        private readonly UserService _userService;
        private readonly List<SpeechCase> SpeechCases;

        public CharmInterpreter(CharmManager manager, ILogger<CharmInterpreter> logger, UserService userService)
        {
            _manager = manager;
            _logger = logger;
            _userService = userService;

            SpeechCases = new List<SpeechCase>
            {
                new TaskListCase(),
                new TaskCreationCase(),
            };
        }

        public async Task<string> TakeMessage(Message message)
        {
            _logger.LogDebug($"Got message: {_userService.UserInfo}");

            var textMessage = message.Text;
            if (textMessage is null)
            {
                _logger.LogError($"Text message was null. - {_userService.UserInfo}");
                return "Не удалось распознать сообщение!";
            }

            if (textMessage.Trim() == "")
            {
                _logger.LogDebug($"Text message was empty. = {_userService.UserInfo}");
                return "Не удалось распознать сообщение!";
            }

            MessageInfo messageInfo = new MessageInfo(textMessage);
            foreach (var speechCase in SpeechCases)
            {
                if (speechCase.TryParse(messageInfo))
                {
                    var result = await speechCase.ApplyAndRespond(_userService.CharmUser.Id, _manager);
                    return result;
                }
            }

            return "Не удалось распознать сообщение!";
        }
    }
}