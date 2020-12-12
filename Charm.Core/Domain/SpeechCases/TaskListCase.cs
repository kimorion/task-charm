using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Charm.Core.Domain.Dto;
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

        public TaskListCase(CharmInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public override bool TryParse(MessageInfo message)
        {
            _interpreter.SetPattern
            (
                @" [ {1}>listTypeParser ] (задачи | дела | (список [{1}>listTypeParser] (задач | дел)) [на {1}>dateParser]) #"
            );
            _interpreter.AddParser("listTypeParser", ListTypeParser);
            _interpreter.AddParser("dateParser", DateParser);

            return _interpreter.TryInterpret(message.OriginalString);
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
            var criteria = new GistSearchCriteria();
            criteria.Date = _date;
            if (_listCreationType == ListCreationType.Undone)
            {
                criteria.IsDone = false;
            }

            var gists = await manager.SearchGists(userId, criteria);

            StringBuilder builder = new StringBuilder();
            builder.Append(_listCreationType == ListCreationType.All
                ? "Список всех задач"
                : "Список несделанных задач");
            builder.Append(_date.HasValue ? " на " + _date.Value.ToString("yyyy-M-d") : " за все время");
            builder.Append(":");
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