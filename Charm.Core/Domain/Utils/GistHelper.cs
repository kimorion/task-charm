using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Charm.Core.Domain.SpeechCases;
using Charm.Core.Infrastructure.Entities;

namespace Charm.Core.Domain.Utils
{
    public static class GistHelper
    {
        public static string CreateGistListResponse(List<Gist> gists)
        {
            StringBuilder responseBuilder = new StringBuilder();
            var i = 1;
            foreach (var gist in gists)
            {
                responseBuilder.Append($"{i}) - ");
                i++;
                string dateTimeString =
                    gist.Reminder?.Deadline.ToString("(dddd HH:mm yyyy-M-d)", CultureInfo.GetCultureInfo("RU-ru")) ??
                    "";
                responseBuilder.AppendLine(
                    $"{(gist.IsDone ? $"<s>{gist.Text}</s>" : $"{gist.Text}")} <i>{dateTimeString}</i>");
            }

            return responseBuilder.ToString();
        }
    }
}