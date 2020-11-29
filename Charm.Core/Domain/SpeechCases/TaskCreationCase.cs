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
            // [в] (сегодня, среду) [в 5] [в 15:00] [вечером] 
            // .ContainsAnyOut(["сегодня", "завтра", "послезавтра"], out DateTime)
            //     .WithNext("в").WithNextOut(_ => shortTimeParser<T, T?>(_), out TimeSpan)
            //     .WithNext("в").WithNextOut(_ => fullTimeParser<T, T?>(_), out TimeSpan)
            //     .WithNextOut(_ => speechTimeParser<T, T?>(_), out TimeSpan)

            var startSearch = message.ContainsAnySingle(CharmParser.DayNames);
            if (!startSearch.IsValid) return false;

            startSearch.Out(CharmParser.ParseDay, out _date);

            startSearch.ToGroup().SkipNext("в").WithNextOut(CharmParser.ParseTime, out _time)
                .Build(out var searchWithTime);

            if (searchWithTime.SkipPrev("в").NotAtTheBeginning().Build(out var searchWithText))
            {
                _text = searchWithText.GetBeginning();
                return true;
            }

            if (searchWithTime.NotAtTheEnd().IsValid)
            {
                _text = searchWithText.GetEnding();
                return true;
            }

            return false;
        }

        public override async Task<string> ApplyAndRespond(long userId, CharmManager manager)
        {
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