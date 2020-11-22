using System;
using System.ComponentModel.DataAnnotations;
using Charm.Core.Infrastructure.Entities.Base;

namespace Charm.Core.Infrastructure.Entities
{
    public class Reminder : IDbEntity<Guid>
    {
        public Guid Id { get; set; }

        [Required]
        public Guid GistId { get; set; }

        public Gist Gist { get; set; } = null!;

        public DateTimeOffset Deadline { get; set; }
        public TimeSpan? Advance { get; set; }
    }
}