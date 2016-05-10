using System;

namespace Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces
{
    public interface ISqlExpressionWalkable
    {
        IQueryExpression Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func);
    }
}