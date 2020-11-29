using System;

namespace Charm.Core.Domain.Dto
{
    public class ReminderRequest
    {
        public Guid GistId { get; set; }
        public DateTimeOffset Deadline { get; set; }
        public TimeSpan? Advance { get; set; }
    }
}