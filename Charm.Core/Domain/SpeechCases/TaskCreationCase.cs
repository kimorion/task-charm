using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Charm.Core.Domain.Dto;
using Charm.Core.Domain.Entities;
using Charm.Core.Domain.Interpreter;
using Charm.Core.Domain.Services;
using Charm.Core.Domain.Utils;
using Telegram.Bot.Types;

namespace Charm.Core.Domain.SpeechCases
{
    public class TaskCreationCase : SpeechCase
    {
        private readonly CharmInterpreter _interpreter;
        private string? _text;
        private DateTimeOffset? _date;
        private TimeSpan? _time;
        private string? _amount;

        public TaskCreationCase(CharmInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public override bool TryParse(MessageInfo message)
        {
            _interpreter.AddParser("dayParser", DayParser);
            _interpreter.AddParser("clockTimeParser", ClockTimeParser);
            _interpreter.AddParser("spanTimeParser", SpanTimeParser);
            _interpreter.AddParser("textParser", TextParser);
            _interpreter.AddParser("amountParser", AmountParser);

            _interpreter.SetPattern
            (
                @"через [{1}>amountParser] {1}>spanTimeParser {*}>textParser #"
            );
            var result = _interpreter.TryInterpret(message.OriginalString);
            if (result) return true;

            _interpreter.SetPattern
            (
                @"[в | во] [{1}>dayParser] [в] [{1}>clockTimeParser] [часов] {*}>textParser #"
            );
            result = _interpreter.TryInterpret(message.OriginalString);


            return result;
        }

        private bool AmountParser(List<string> words)
        {
            if (words.Count != 1) return false;
            if (!int.TryParse(words[0], out var amount)) return false;
            _amount = words[0];
            return true;
        }

        private bool SpanTimeParser(List<string> words)
        {
            if (words.Count != 1) return false;

            if (_amount == null)
            {
                _time = CharmParser.ParseShortTimeSpan(words[0]);
                return _time != null;
            }

            words.Insert(0, _amount);
            _time = CharmParser.ParseSpanTime(words);

            return _time != null;
        }

        private bool DayParser(List<string> words)
        {
            if (words.Count != 1) return false;

            _date = CharmParser.ParseDay(words[0]);
            return _date != null;
        }

        private bool ClockTimeParser(List<string> words)
        {
            if (words.Count != 1) return false;

            _time = CharmParser.ParseClockTime(words[0]);
            return _time != null;
        }

        private bool TextParser(List<string> words)
        {
            if (words.Count <= 0) return false;

            _text = string.Join(" ", words);
            return true;
        }

        public override async Task<string> ApplyAndRespond(long userId, CharmManager manager)
        {
            if (_text is null) throw new Exception();

            if (_time.HasValue)
            {
                var currentTime = DateTimeOffset.Now;
                DateTimeOffset notificationDate = currentTime.DateTime.Date;
                if (currentTime.TimeOfDay >= _time.Value)
                {
                    notificationDate = notificationDate.AddDays(1);
                }

                _date ??= notificationDate;
                _date = _date.Value.Add(_time.Value);
            }
            else if (_date.HasValue)
            {
                _date = _date.Value.AddHours(12);
            }

            if (_date is not null)
            {
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