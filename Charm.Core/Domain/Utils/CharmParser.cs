using System;
using System.Collections.Generic;
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
            return word.Value switch
            {
                "сегодня" => DateTimeOffset.Now,
                "завтра" => DateTimeOffset.Now,
                "послезавтра" => DateTimeOffset.Now,
                "понедельник" => DateTimeOffset.Now,
                "вторник" => DateTimeOffset.Now,
                "среду" => DateTimeOffset.Now,
                "четверг" => DateTimeOffset.Now,
                "пятницу" => DateTimeOffset.Now,
                "субботу" => DateTimeOffset.Now,
                "воскресенье" => DateTimeOffset.Now,
                _ => null
            };
        }

        public static TimeSpan? ParseTime(Word arg)
        {
            throw new NotImplementedException();
        }
    }
}