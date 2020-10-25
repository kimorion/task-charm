using System;
using Charm.Core.Infrastructure.Entities.Base;

namespace Charm.Core.Infrastructure.Entities
{
    public class Task : IDbEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Gist { get; set; }
        public User User { get; set; }
        public Guid ReminderId { get; set; }
        public Reminder Reminder { get; set; }
        public Guid ParentTaskId { get; set; }
        public Task ParentTask { get; set; }
    }
}