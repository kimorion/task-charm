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
            "вчера",
            "послезавтра",
            "понедельник",
            "вторник",
            "среду",
            "четверг",
            "пятницу",
            "субботу",
            "воскресенье",
        };

        public static DateTimeOffset? ParseDay(string s)
        {
            var today = DateTime.Today;
            return s switch
            {
                "сегодня" => today,
                "завтра" => today.AddDays(1),
                "вчера" => today.AddDays(-1),
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

        public static TimeSpan? ParseClockTime(string s)
        {
            string[] validFormats = {@"hh\:mm", "%h"};
            if (TimeSpan.TryParseExact(s, validFormats, CultureInfo.InvariantCulture, out var time))
            {
                return time;
            }

            return null;
        }

        public static TimeSpan? ParseSpanTime(List<string> words)
        {
            if (!int.TryParse(words[0], out var number)) return null;
            if (!TryParseTimeUnit(words[1], out var unitType)) return null;

            TimeSpan? addedTime = unitType switch
            {
                TimeUnitType.Second => TimeSpan.FromSeconds(number),
                TimeUnitType.Minute => TimeSpan.FromMinutes(number),
                TimeUnitType.Hour => TimeSpan.FromHours(number),
                TimeUnitType.Day => TimeSpan.FromDays(number),
                _ => null
            };

            return addedTime;
        }

        public static TimeSpan? ParseShortTimeSpan(string s)
        {
            TimeUnitType? timeUnitType = s switch
            {
                "секунду" => TimeUnitType.Second,
                "минуту" => TimeUnitType.Minute,
                "час" => TimeUnitType.Hour,
                "день" => TimeUnitType.Day,
                _ => null
            };

            if (timeUnitType == null) return null;

            return timeUnitType switch
            {
                TimeUnitType.Second => TimeSpan.FromSeconds(1),
                TimeUnitType.Minute => TimeSpan.FromMinutes(1),
                TimeUnitType.Hour => TimeSpan.FromHours(1),
                TimeUnitType.Day => TimeSpan.FromDays(1),
                _ => throw new Exception("impossible timeUnitType value")
            };
        }

        public static bool TryParseTimeUnit(string s, out TimeUnitType? unitType)
        {
            unitType = s switch
            {
                "секунд" => TimeUnitType.Second,
                "секунды" => TimeUnitType.Second,
                "секунду" => TimeUnitType.Second,
                "минут" => TimeUnitType.Minute,
                "минуты" => TimeUnitType.Minute,
                "минуту" => TimeUnitType.Minute,
                "часов" => TimeUnitType.Hour,
                "час" => TimeUnitType.Hour,
                "день" => TimeUnitType.Day,
                "дня" => TimeUnitType.Day,
                "дней" => TimeUnitType.Day,
                _ => null
            };

            return unitType != null;
        }

        public static bool NumberParser(List<string> words, out List<int>? numbers)
        {
            numbers = new List<int>();
            foreach (var word in words)
            {
                if (!int.TryParse(word, out var number))
                {
                    return false;
                }

                numbers.Add(number);
            }

            if (numbers.Count <= 0)
            {
                numbers = null;
            }

            return numbers != null;
        }
    }

    public enum TimeUnitType
    {
        Second,
        Minute,
        Hour,
        Day,
    }
}