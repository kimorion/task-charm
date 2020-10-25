using System;
using Charm.Core.Infrastructure.Entities.Base;

namespace Charm.Core.Infrastructure.Entities
{
    public class Reminder : IDbEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Task Task { get; set; }
        public DateTimeOffset Deadline { get; set; }
        public DateTimeOffset Advance { get; set; }
    }
}