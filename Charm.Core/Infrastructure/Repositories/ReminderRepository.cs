using System;
using Charm.Core.Infrastructure.Entities;

namespace Charm.Core.Infrastructure.Repositories
{
    public class ReminderRepository : Repository<Reminder, Guid, CharmDbContext>
    {
        public ReminderRepository(CharmDbContext context) : base(context)
        {
        }
    }
}