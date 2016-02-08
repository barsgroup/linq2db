namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IExpr<T1, out T2>
        where T1 : IConditionBase<T1, T2>
    {
        IOperator<T2> Equal { get; }

        IOperator<T2> NotEqual { get; }

        IOperator<T2> Greater { get; }

        IOperator<T2> GreaterOrEqual { get; }

        IOperator<T2> NotGreater { get; }

        IOperator<T2> Less { get; }

        IOperator<T2> LessOrEqual { get; }

        IOperator<T2> NotLess { get; }

        ISqlExpression SqlExpression { get; }

        T2 IsNull { get; }

        T2 IsNotNull { get; }

        T2 Like(ISqlExpression expression, SqlValue escape);

        T2 Like(ISqlExpression expression);

        T2 Like(string expression,         SqlValue escape);

        T2 Like(string expression);

        T2 Between   (ISqlExpression expr1, ISqlExpression expr2);

        T2 NotBetween(ISqlExpression expr1, ISqlExpression expr2);

        T2 In   (ISelectQuery subQuery);

        T2 NotIn(ISelectQuery subQuery);

        T2 In   (params object[] exprs);

        T2 NotIn(params object[] exprs);

        T2 Add(ISqlPredicate predicate);

    }
}