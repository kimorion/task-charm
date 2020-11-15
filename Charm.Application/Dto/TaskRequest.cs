using System;

namespace Charm.Application.Dto
{
    public class TaskRequest
    {
        public string Message { get; set; }
        public DateTimeOffset Deadline { get; set; }
        public DateTimeOffset Advance { get; set; }
    }
}