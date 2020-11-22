using System;
using System.ComponentModel.DataAnnotations;
using Charm.Core.Infrastructure.Entities.Base;

namespace Charm.Core.Infrastructure.Entities
{
    public class Gist : IDbEntity<Guid>
    {
        public Guid Id { get; set; }

        [Required]
        public string Text { get; set; } = "";

        [Required]

        public Guid UserId { get; set; }

        public User User { get; set; } = null!;
        public Reminder? Reminder { get; set; }
        public Guid? ParentGistId { get; set; }
        public Gist? ParentGist { get; set; }
    }
}