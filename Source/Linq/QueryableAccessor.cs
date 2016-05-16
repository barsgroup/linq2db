using System;
using System.Linq;
using System.Linq.Expressions;

namespace Bars2Db.Linq
{
    internal class QueryableAccessor
    {
        public Func<Expression, IQueryable> Accessor;
        public IQueryable Queryable;
    }
}