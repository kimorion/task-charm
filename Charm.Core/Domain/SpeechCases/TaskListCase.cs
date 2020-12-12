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
                // @"раз два три [четыре] пять"
                @" [ {1}>listTypeParser ] (задачи | дела | (список [{1}>listTypeParser] (задач | дел)) [на {1}>dateParser]) #"
                // @" [ {1}>listTypeParser ] задачи | дела | список [{1}>listTypeParser] (задач | дел) [на {1}>dateParser] #"
                // @"| & !"
                // @"[раз два три | четыре] (раз | два | три четыре) | &"
                // @"[раз два три | четыре] & (раз | два | три четыре) | &"
                // @"& [& раз | два | три] | (раз два три)"
                // @""
                // @"[раз два три | четыре пять] (раз два три)"
                // @""
            );
            _interpreter.AddParser("listTypeParser", ListTypeParser);
            _interpreter.AddParser("dateParser", DateParser);

            _result = _interpreter.TryInterpret(message.OriginalString);
            return true;
        }

        private bool DateParser(List<string> words)
        {
            if (words.Count != 1)
            {
                return false;
            }

            var s = words[0];
            _date = CharmParser.ParseDay(s);
            return _date != null;
        }

        private bool ListTypeParser(List<string> words)
        {
            if (words.Count != 1)
            {
                return false;
            }

            var s = words[0];
            if (s == "все" || s == "всех")
            {
                _listCreationType = ListCreationType.All;
                return true;
            }

            return false;
        }

        public override async Task<string> ApplyAndRespond(long userId, CharmManager manager)
        {
            return _result ? $"{_listCreationType} {_date}" : "Couldn't match!";

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