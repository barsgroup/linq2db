using System;
using System.Collections.Generic;
using System.Linq;

namespace Bars2Db.Linq.Joiner.Interfaces
{
    public interface ISmartJoiner : IQueryProvider
    {
        IQueryable<TEntity> CreateQuery<TEntity>();

        IQueryable CreateQuery(Type resultType);

        IEnumerable<TEntity> Execute<TEntity>(IQueryable<TEntity> smartQuery);
    }
}