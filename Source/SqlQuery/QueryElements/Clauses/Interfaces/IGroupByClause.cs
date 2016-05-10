using System.Collections.Generic;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Clauses.Interfaces
{
    public interface IGroupByClause : ISqlExpressionWalkable, IClauseWithConditionBase
    {
        [SearchContainer]
        LinkedList<IQueryExpression> Items { get; }

        bool IsEmpty { get; }
        IGroupByClause Expr(IQueryExpression expr);
    }
}