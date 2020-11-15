using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Charm.Core.Infrastructure.Entities.Base;
using Charm.Core.Infrastructure.Exceptions;
using Charm.Core.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Charm.Core.Infrastructure.Repositories
{
    public class Repository<TEntity, TKey, TContext>
        where TEntity : class, IDbEntity<TKey>
        where TContext : DbContext, IContext<TEntity>
    {
        protected readonly TContext Context;

        public Repository(TContext context)
        {
            Context = context;
        }

        public virtual async Task<Option<Exception>> Add(TEntity entity)
        {
            try
            {
                Context.GetDbSet.Add(entity);
                await Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return Option.Some(e);
            }

            return Option.None<Exception>();
        }

        public virtual Task<Option<TEntity, Exception>> Get(TKey id)
            => SearchSingle(q => q.Where(e => e.Id.Equals(id)));

        public virtual async Task<Option<TEntity, Exception>> Update(TKey id, TEntity entity)
        {
            try
            {
                Context.GetDbSet.Update(entity);
                await Context.SaveChangesAsync();
                return Option.Some<TEntity, Exception>(entity);
            }
            catch (Exception e)
            {
                return Option.None<TEntity, Exception>(e);
            }
        }

        public virtual async Task<Option<TEntity, Exception>> Delete(TKey id)
        {
            try
            {
                var entity = await SearchSingle(q => q.Where(e => e.Id.Equals(id)));
                entity.Map(e =>
                {
                    Context.GetDbSet.Remove(e);
                    await Context.SaveChangesAsync();
                    return Option.Some<TEntity, Exception>(entity.valu);
                })
                
            }
            catch (Exception e)
            {
                return Option.None<TEntity, Exception>(e);
            }
        }

        public virtual async Task<Option<TEntity, Exception>> SearchSingle(
            Func<IQueryable<TEntity>, IQueryable<TEntity>> searchQuery)
        {
            try
            {
                var query = searchQuery(Context.GetDbSet);
                return
                    (await query.SingleOrDefaultAsync())
                    .SomeNotNull(
                        DatabaseExceptions.NotFound());
            }
            catch (InvalidOperationException e)
            {
                return Option.None<TEntity, Exception>(DatabaseExceptions.NotSingle());
            }
            catch (Exception e)
            {
                return Option.None<TEntity, Exception>(e);
            }
        }

        public virtual async Task<Option<ICollection<TEntity>, Exception>> SearchRange(
            Func<IQueryable<TEntity>, IQueryable<TEntity>> searchQuery)
        {
            try
            {
                var query = searchQuery(Context.GetDbSet);
                var entityList = await query.ToListAsync();
                return Option.Some<ICollection<TEntity>, Exception>(entityList);
            }
            catch (Exception e)
            {
                return Option.None<ICollection<TEntity>, Exception>(e);
            }
        }
    }
}