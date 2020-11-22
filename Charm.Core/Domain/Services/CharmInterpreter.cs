using System;
using System.Threading.Tasks;
using Charm.Core.Domain.Dto;

namespace Charm.Core.Domain.Services
{
    public class CharmInterpreter
    {
        private readonly CharmManager _manager;

        public CharmInterpreter(CharmManager manager)
        {
            _manager = manager;
        }

        public async Task<string> TakeTextMessage(long chatId, string message)
        {
            await _manager.CreateGistWithReminder(new GistWithReminderRequest
            {
                GistMessage = message,
                ChatId = chatId,
                Deadline = DateTimeOffset.Now,
                Advance = TimeSpan.Zero
            });

            return "Задача создана!";
        }
    }
}