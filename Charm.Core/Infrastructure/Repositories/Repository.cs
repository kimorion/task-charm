using System;
using System.Linq;
using System.Threading.Tasks;
using Charm.Core.Infrastructure.Entities.Base;
using Charm.Core.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Charm.Core.Infrastructure.Repositories
{
    public class Repository<TEntity, TKey, TContext> where TEntity : class, IDbEntity<TKey>
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

        public virtual async Task<Option<TEntity, Exception>> Get(TKey id)
        {
            try
            {
                var entity = await Context.GetDbSet.SingleAsync(e => e.Id.Equals(id));
                return Option.Some<TEntity, Exception>(entity);
            }
            catch (Exception e)
            {
                return Option.None<TEntity, Exception>(e);
            }
        }

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
                var entity = await Context.GetDbSet.SingleAsync(e => e.Id.Equals(id));
                Context.GetDbSet.Remove(entity);
                await Context.SaveChangesAsync();
                return Option.Some<TEntity, Exception>(entity);
            }
            catch (Exception e)
            {
                return Option.None<TEntity, Exception>(e);
            }
        }
    }
}