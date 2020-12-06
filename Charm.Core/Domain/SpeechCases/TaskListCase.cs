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

        private ListCreationType _listCreationType = ListCreationType.Undone;
        private DateTimeOffset? _date;
        private bool _result = false;

        public TaskListCase(CharmInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public override bool TryParse(MessageInfo message)
        {
            _interpreter.SetPattern
            (
                @" [ {1}>listTypeParser ] (задачи || дела || (список [{1}>listTypeParser] (задач || дел)) [на {1}>dateParser]) #"
            );
            _interpreter.AddParser("listTypeParser", ListTypeParser);
            _interpreter.AddParser("dateParser", DateParser);

            _result = _interpreter.TryInterpret(message.OriginalString);
            return true;
        }

        private bool DateParser(string str)
        {
            _date = CharmParser.ParseDay(str);
            return _date != null;
        }

        private bool ListTypeParser(string s)
        {
            if (s == "все" || s == "всех")
            {
                _listCreationType = ListCreationType.All;
                return true;
            }

            return false;
        }

        public override async Task<string> ApplyAndRespond(long userId, CharmManager manager)
        {
            // return _result ? "Matched!" : "Couldn't match!";

            var gists = await manager.Context.Gists
                .Where(g => _listCreationType == ListCreationType.All || g.IsDone).Where(g => g.UserId == userId)
                .Include(g => g.Reminder)
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
            All,
            Undone
        }
    }
}