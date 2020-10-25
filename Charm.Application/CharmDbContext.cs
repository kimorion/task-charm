using Charm.Core.Infrastructure.Entities;
using Charm.Core.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Charm.Application
{
    public class CharmDbContext : DbContext, IContext<Task>, IContext<Reminder>
    {
        public CharmDbContext(DbContextOptions<CharmDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Task> Tasks { get; set; }
        public DbSet<Reminder> Reminders { get; set; }

        DbSet<Task> IContext<Task>.GetDbSet => Tasks;

        DbSet<Reminder> IContext<Reminder>.GetDbSet => Reminders;
    }
}