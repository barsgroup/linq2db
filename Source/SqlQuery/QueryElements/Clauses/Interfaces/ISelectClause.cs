namespace LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface ISelectClause : IClauseWithConditionBase, ISqlExpressionWalkable
    {
        void Expr(IQueryExpression expr);

        int Add(IQueryExpression expr);

        int Add(IQueryExpression expr, string alias);

        [SearchContainer]
        List<IColumn> Columns { get; }

        bool HasModifier { get; }

        bool IsDistinct { get; set; }

        [SearchContainer]
        IQueryExpression TakeValue { get; set; }

        [SearchContainer]
        IQueryExpression SkipValue { get; set; }
    }
}