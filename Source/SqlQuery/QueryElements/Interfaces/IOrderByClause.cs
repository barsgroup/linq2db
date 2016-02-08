namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.Clauses;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IOrderByClause : ISqlExpressionWalkable, IQueryElement
    {
        IOrderByClause Expr(ISqlExpression expr, bool isDescending);

        IOrderByClause Expr     (ISqlExpression expr);

        IOrderByClause ExprAsc  (ISqlExpression expr);

        IOrderByClause ExprDesc (ISqlExpression expr);

        IOrderByClause Field    (SqlField field, bool isDescending);

        IOrderByClause Field    (SqlField field);

        IOrderByClause FieldAsc (SqlField field);

        IOrderByClause FieldDesc(SqlField field);

        List<IOrderByItem> Items { get; }

        bool IsEmpty { get; }

        ISelectClause Select { get; }

        IFromClause From { get; }

        WhereClause Where { get; }

        GroupByClause GroupBy { get; }

        WhereClause Having { get; }

        IOrderByClause OrderBy { get; }

        ISelectQuery SelectQuery { get; }

        ISelectQuery End();

        void SetSqlQuery(ISelectQuery selectQuery);
    }
}