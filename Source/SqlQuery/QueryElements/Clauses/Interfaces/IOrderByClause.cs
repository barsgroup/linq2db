namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.Clauses;
    using LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IOrderByClause : ISqlExpressionWalkable, IQueryElement
    {
        IOrderByClause Expr(IQueryExpression expr, bool isDescending);

        IOrderByClause Expr     (IQueryExpression expr);

        IOrderByClause ExprAsc  (IQueryExpression expr);

        IOrderByClause ExprDesc (IQueryExpression expr);

        IOrderByClause Field    (ISqlField field, bool isDescending);

        IOrderByClause Field    (ISqlField field);

        IOrderByClause FieldAsc (ISqlField field);

        IOrderByClause FieldDesc(ISqlField field);

        List<IOrderByItem> Items { get; }

        bool IsEmpty { get; }

        ISelectClause Select { get; }

        IFromClause From { get; }

        IWhereClause Where { get; }

        GroupByClause GroupBy { get; }

        IWhereClause Having { get; }

        IOrderByClause OrderBy { get; }

        ISelectQuery SelectQuery { get; }

        ISelectQuery End();

        void SetSqlQuery(ISelectQuery selectQuery);
    }
}