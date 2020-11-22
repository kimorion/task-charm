using System;

namespace Charm.Core.Domain.Dto
{
    public class GistWithReminderRequest
    {
        public Guid UserId { get; set; }
        public string GistMessage { get; set; } = "";
        public DateTimeOffset Deadline { get; set; }
        public TimeSpan? Advance { get; set; }
    }
}