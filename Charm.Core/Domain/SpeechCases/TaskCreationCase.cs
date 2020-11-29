using System;
using System.Globalization;
using System.Threading.Tasks;
using Charm.Core.Domain.Dto;
using Charm.Core.Domain.Entities;
using Charm.Core.Domain.Services;
using Charm.Core.Domain.Utils;
using Telegram.Bot.Types;

namespace Charm.Core.Domain.SpeechCases
{
    public class TaskCreationCase : SpeechCase
    {
        private string? _text;
        private DateTimeOffset? _date;
        private TimeSpan? _time;

        public override bool TryParse(MessageInfo message)
        {
            var daySearch = message.SearchAnySingle(CharmParser.DayNames);
            if (!daySearch.IsValid)
            {
                _text = message.OriginalString;
                return true;
            }

            var trashWords = new[] {"в", "во", "вечером", "утром", "днем", "днём"};
            
            if (daySearch.Out(CharmParser.ParseDay, out _date).ToGroupSearch().Build(out var dateSearch))
            {
                dateSearch = dateSearch
                    // .SkipAnyPrev(new[] {"в", "во"})
                    // .SkipAnyNext(new[] {"вечером", "утром", "днем", "днём"});
                    .SkipAnyPrev(trashWords)
                    .SkipAnyNext(trashWords);
            }
            else
            {
                dateSearch = daySearch.ToGroupSearch();
            }

            if (dateSearch
                .SkipNext("в")
                .WithNextOut(CharmParser.ParseTime, out _time)
                .SkipAnyNext(new[] {"вечером", "утром", "днем", "днём"})
                .Build(out var dateTimeSearch))
            {
                _text = dateTimeSearch.GetEnding() ?? dateTimeSearch.GetBeginning();
                return true;
            }

            _text = dateSearch.GetEnding() ?? dateSearch.GetBeginning();
            return _text != null;
        }

        public override async Task<string> ApplyAndRespond(long userId, CharmManager manager)
        {
            return $"TaskCreation {_text} {_date} {_time}";
            if (_text is null) throw new Exception();

            if (_date is not null)
            {
                if (_time is not null)
                {
                    _date = _date.Value.Add(_time.Value);
                }

                await manager.CreateGistWithReminder(new GistWithReminderRequest
                {
                    ChatId = userId,
                    Deadline = _date.Value,
                    GistMessage = _text
                });

                return
                    $"Создана задача \"{_text}\" на " +
                    $"{_date.Value.ToString("yyyy-M-d dddd HH:mm", CultureInfo.GetCultureInfo("RU-ru"))}";
            }
            else
            {
                await manager.CreateGist(new GistRequest {ChatId = userId, GistMessage = _text});
                return $"Создана задача \"{_text}\" без времени";
            }
        }
    }
}