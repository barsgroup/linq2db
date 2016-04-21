namespace LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IGroupByClause : ISqlExpressionWalkable, IClauseWithConditionBase
    {
        IGroupByClause Expr(IQueryExpression expr);

        [SearchContainer]
        LinkedList<IQueryExpression> Items { get; }

        bool IsEmpty { get; }

    }
}