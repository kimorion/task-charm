#nullable disable
using System.Linq;
using Charm.Core.Infrastructure.Database.Base;
using Charm.Core.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Charm.Core.Infrastructure
{
    public class CharmDbContext : DbContext
    {
#nullable restore
        public DbSet<Gist> Gists { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ChatContext> ChatContexts { get; set; }
#nullable disable

        public CharmDbContext(DbContextOptions<CharmDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Gist>()
                .HasOne(t => t.Reminder)
                .WithOne(r => r.Gist)
                .HasForeignKey<Reminder>(t => t.GistId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reminder>()
                .HasIndex(r => r.Deadline);

            modelBuilder.Entity<ChatContext>().HasNoKey();

            base.OnModelCreating(modelBuilder);
        }
    }
}