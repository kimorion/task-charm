using System;
using Charm.Core.Infrastructure.Entities.Base;
using Charm.Core.Infrastructure.Repositories.Base;
using Optional;

namespace Charm.Core.Infrastructure.Repositories
{
    public class Repository<TEntity, TKey, TContext> where TEntity : IDbEntity<TKey>
        where TContext : IContext<IDbEntity<TKey>>
    {
        protected readonly IContext<IDbEntity<TKey>> Context;

        public Repository(IContext<IDbEntity<TKey>> context)
        {
            Context = context;
        }

        public Option<Exception> Add(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public Option<TEntity, Exception> Get(TKey id)
        {
            throw new NotImplementedException();
        }

        public Option<Exception> Update(TKey id, TEntity entity)
        {
            throw new NotImplementedException();
        }

        public Option<Exception> Delete(TKey id)
        {
            throw new NotImplementedException();
        }
    }
}