﻿using Microsoft.EntityFrameworkCore;

namespace Charm.Core.Infrastructure.Repositories.Base
{
    public interface IContext<TEntity> where TEntity : class
    {
        DbSet<TEntity> GetDbSet { get; }
    }
}