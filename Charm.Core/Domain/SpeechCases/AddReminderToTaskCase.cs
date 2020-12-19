using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Charm.Core.Domain.Dto;
using Charm.Core.Domain.Entities;
using Charm.Core.Domain.Interpreter;
using Charm.Core.Domain.Services;
using Charm.Core.Domain.Utils;
using Charm.Core.Infrastructure.Entities;

namespace Charm.Core.Domain.SpeechCases
{
    public class AddReminderToTaskCase : SpeechCase
    {
        private readonly CharmInterpreter _interpreter;
        private List<int>? _numbers;
        private DateTimeOffset? _date;
        private TimeSpan? _time;
        private string? _amount;

        public AddReminderToTaskCase(CharmInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public override bool TryParse(MessageInfo message)
        {
            _interpreter.AddParser("numberParser", NumberParser);
            _interpreter.AddParser("dayParser", DayParser);
            _interpreter.AddParser("clockTimeParser", ClockTimeParser);
            _interpreter.AddParser("amountParser", AmountParser);
            _interpreter.AddParser("spanTimeParser", SpanTimeParser);


            _interpreter.SetPattern
            (
                @"[создай | добавь] напоминание | напомни [о | про] {1}>numberParser
                    [в | во | на] [{1}>dayParser] [в] [{1}>clockTimeParser] [часов] #"
            );
            var result = _interpreter.TryInterpret(message.OriginalString);

            if (!result)
            {
                _interpreter.SetPattern
                (
                    @"[создай | добавь] напоминание | напомни [о | про] {1}>numberParser
                    через [{1}>amountParser] {1}>spanTimeParser #"
                );
                result = _interpreter.TryInterpret(message.OriginalString);
            }

            return !(_numbers is null || _date is null && _time is null) && result;
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

        private bool NumberParser(List<string> words)
        {
            return CharmParser.NumberParser(words, out _numbers);
        }

        public override async Task<string> ApplyAndRespond(long userId, CharmManager manager)
        {
            if (_numbers == null) throw new NullReferenceException(nameof(_numbers));

            if (_time.HasValue)
            {
                _date ??= DateTimeOffset.Now;
                _date = _date.Value.Add(_time.Value);
            }

            if (_date == null) throw new NullReferenceException(nameof(_date));

            Gist? gist = (await manager.GetGistsFromContext(new List<int> {_numbers[0]})).FirstOrDefault();
            if (gist == null) return "Задача не найдена, обновите список";
            if (gist.Reminder != null)
            {
                gist.Reminder.Deadline = _date.Value;
                await manager.Context.SaveChangesAsync();
                return "Напоминание было обновлено для задачи:\n " +
                       GistHelper.CreateGistListResponse(new List<Gist> {gist});
            }

            await manager.CreateReminder(new ReminderRequest
            {
                GistId = gist.Id,
                Deadline = _date.Value
            });

            return "Напоминание было создано для задачи: \n" +
                   GistHelper.CreateGistListResponse(new List<Gist> {gist});
        }
    }
}