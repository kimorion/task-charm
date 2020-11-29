using System;
using System.Collections.Generic;
using System.Globalization;
using Charm.Core.Domain.Entities;

namespace Charm.Core.Domain.Utils
{
    public static class CharmParser
    {
        public static List<string> DayNames = new List<string>
        {
            "сегодня",
            "завтра",
            "послезавтра",
            "понедельник",
            "вторник",
            "среду",
            "четверг",
            "пятницу",
            "субботу",
            "воскресенье",
        };

        public static DateTimeOffset? ParseDay(Word word)
        {
            var today = DateTime.Today;
            return word.Value switch
            {
                "сегодня" => today,
                "завтра" => today.AddDays(1),
                "послезавтра" => today.AddDays(2),
                "понедельник" => GetNextWeekday(today, DayOfWeek.Monday),
                "вторник" => GetNextWeekday(today, DayOfWeek.Tuesday),
                "среду" => GetNextWeekday(today, DayOfWeek.Wednesday),
                "четверг" => GetNextWeekday(today, DayOfWeek.Thursday),
                "пятницу" => GetNextWeekday(today, DayOfWeek.Friday),
                "субботу" => GetNextWeekday(today, DayOfWeek.Saturday),
                "воскресенье" => GetNextWeekday(today, DayOfWeek.Sunday),
                _ => null
            };
        }

        private static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int) day - (int) start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        public static TimeSpan? ParseTime(Word word)
        {
            string[] validFormats = {@"hh\:mm", "%h"};
            if (TimeSpan.TryParseExact(word.Value, validFormats, CultureInfo.InvariantCulture, out var time))
            {
                return time;
            }

            return null;
        }
    }
}