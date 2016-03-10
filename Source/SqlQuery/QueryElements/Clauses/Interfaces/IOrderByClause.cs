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

        IOrderByClause Expr     (IQueryExpression expr);

        IOrderByClause ExprAsc  (IQueryExpression expr);

        IOrderByClause ExprDesc (IQueryExpression expr);

        IOrderByClause Field    (ISqlField field, bool isDescending);

        IOrderByClause Field    (ISqlField field);

        IOrderByClause FieldAsc (ISqlField field);

        IOrderByClause FieldDesc(ISqlField field);

        [SearchContainer]
        List<IOrderByItem> Items { get; }

        bool IsEmpty { get; }
    }
}