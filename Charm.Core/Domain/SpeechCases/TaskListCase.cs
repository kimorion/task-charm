using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Charm.Core.Domain.Entities;
using Charm.Core.Domain.Interpreter;
using Charm.Core.Domain.Services;
using Charm.Core.Domain.Utils;
using Microsoft.EntityFrameworkCore;

namespace Charm.Core.Domain.SpeechCases
{
    public class TaskListCase : SpeechCase
    {
        private readonly CharmInterpreter _interpreter;

        private ListCreationType? _listCreationType = null!;
        private DateTimeOffset? _date;
        private bool result = false;

        public TaskListCase(CharmInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public override bool TryParse(MessageInfo message)
        {
            _interpreter.SetPattern
            (
                @"задачи || таски || (список (задач || тасков)) тест"
            );

            result = _interpreter.TryInterpret(message.OriginalString);
            return true;
        }

        public override async Task<string> ApplyAndRespond(long userId, CharmManager manager)
        {
            return result ? "Matched!" : "Couldn't match!";

            var gists = await manager.Context.Gists.Where(g => g.UserId == userId).Include(g => g.Reminder)
                .ToListAsync();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Все созданные задачи:");
            builder.AppendLine();
            var i = 1;
            foreach (var gist in gists)
            {
                builder.Append($"{i++}. ");
                string dateTimeString =
                    gist.Reminder?.Deadline.ToString("yyyy-M-d dddd HH:mm", CultureInfo.GetCultureInfo("RU-ru")) ??
                    "без времени";
                builder.AppendLine(
                    $"{gist.Text} ({(gist.IsDone ? "X" : "0")}) ({dateTimeString})");
            }

            return builder.ToString();
        }

        enum ListCreationType
        {
            Done,
            Undone
        }
    }
}