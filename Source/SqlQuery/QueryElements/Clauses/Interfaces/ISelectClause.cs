using System.Collections.Generic;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Clauses.Interfaces
{
    public interface ISelectClause : IClauseWithConditionBase, ISqlExpressionWalkable
    {
        [SearchContainer]
        List<IColumn> Columns { get; }

        bool HasModifier { get; }

        bool IsDistinct { get; set; }

        [SearchContainer]
        IQueryExpression TakeValue { get; set; }

        [SearchContainer]
        IQueryExpression SkipValue { get; set; }

        void Expr(IQueryExpression expr);

        int Add(IQueryExpression expr);

        int Add(IQueryExpression expr, string alias);
    }
}