namespace LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.Clauses;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IOrderByClause : ISqlExpressionWalkable, IClauseWithConditionBase
    {
        IOrderByClause Expr(IQueryExpression expr, bool isDescending);

        IOrderByClause ExprAsc  (IQueryExpression expr);

        [SearchContainer]
        List<IOrderByItem> Items { get; }

        bool IsEmpty { get; }
    }
}