namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ISelectClause : IClauseWithConditionBase, ISqlExpressionWalkable
    {
        ISelectClause Expr(IQueryExpression expr);

        int Add(IQueryExpression expr);

        int Add(IQueryExpression expr, string alias);

        List<IColumn> Columns { get; }

        bool HasModifier { get; }

        ISelectClause Distinct { get; }

        bool IsDistinct { get; set; }

        IQueryExpression TakeValue { get; set; }

        IQueryExpression SkipValue { get; set; }

        ISelectClause Take(int value);

        ISelectClause Take(IQueryExpression value);

        ISelectClause Skip(int value);

        ISelectClause Skip(IQueryExpression value);

    }
}