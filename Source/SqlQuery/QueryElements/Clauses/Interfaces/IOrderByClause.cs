using System.Collections.Generic;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Clauses.Interfaces
{
    public interface IOrderByClause : ISqlExpressionWalkable, IClauseWithConditionBase
    {
        [SearchContainer]
        List<IOrderByItem> Items { get; }

        bool IsEmpty { get; }
        IOrderByClause Expr(IQueryExpression expr, bool isDescending);

        IOrderByClause ExprAsc(IQueryExpression expr);
    }
}