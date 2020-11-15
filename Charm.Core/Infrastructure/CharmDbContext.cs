using System.Linq;
using Charm.Core.Infrastructure.Entities;
using Charm.Core.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Charm.Core.Infrastructure
{
    public class CharmDbContext : DbContext, IContext<Task>, IContext<Reminder>, IChangeTrackingContext
    {
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Reminder> Reminders { get; set; }

        public CharmDbContext(DbContextOptions<CharmDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Task>()
                .HasOne(t => t.Reminder)
                .WithOne(r => r.Task)
                .HasForeignKey<Reminder>(t => t.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reminder>()
                .HasOne(r => r.Task)
                .WithOne(t => t.Reminder).HasForeignKey<Task>(r => r.ReminderId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Reminder>()
                .HasIndex(r => r.Deadline);

            base.OnModelCreating(modelBuilder);
        }

        DbSet<Task> IContext<Task>.GetDbSet => Tasks;

        DbSet<Reminder> IContext<Reminder>.GetDbSet => Reminders;

        public void DetachAllEntities()
        {
            var changedEntriesCopy = this.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;
        }
    }
}