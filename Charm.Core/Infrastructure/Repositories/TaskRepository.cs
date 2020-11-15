using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Optional;
using Task = Charm.Core.Infrastructure.Entities.Task;

namespace Charm.Core.Infrastructure.Repositories
{
    public class TaskRepository : Repository<Task, Guid, CharmDbContext>
    {
        public TaskRepository(CharmDbContext context) : base(context)
        {
        }

        public override Task<Option<Task, Exception>> Get(Guid id)
            =>
                SearchSingle(
                    q => q
                        .Where(t => t.Id.Equals(id))
                        .Include(t => t.Reminder));


        public virtual Task<Option<ICollection<Task>, Exception>> GetAllForUser(Guid id)
            =>
                SearchRange(query => query.Where(t => t.Id.Equals(id)));
    }
}