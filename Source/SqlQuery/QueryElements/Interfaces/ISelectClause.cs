namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ISelectClause : IClauseBase, ISqlExpressionWalkable
    {
        ISelectClause Expr(ISqlExpression expr);

        int Add(ISqlExpression expr);

        int Add(ISqlExpression expr, string alias);

        List<IColumn> Columns { get; }

        bool HasModifier { get; }

        ISelectClause Distinct { get; }

        bool IsDistinct { get; set; }

        ISqlExpression TakeValue { get; set; }

        ISqlExpression SkipValue { get; set; }

        ISelectClause Take(int value);

        ISelectClause Take(ISqlExpression value);

        ISelectClause Skip(int value);

        ISelectClause Skip(ISqlExpression value);

    }
}