namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
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

        IQueryExpression SqlExpression { get; }

        T2 IsNull { get; }

        T2 IsNotNull { get; }

        T2 Like(IQueryExpression expression, ISqlValue escape);

        T2 Like(IQueryExpression expression);

        T2 Like(string expression, ISqlValue escape);

        T2 Like(string expression);

        T2 Between   (IQueryExpression expr1, IQueryExpression expr2);

        T2 NotBetween(IQueryExpression expr1, IQueryExpression expr2);

        T2 In   (ISelectQuery subQuery);

        T2 NotIn(ISelectQuery subQuery);

        T2 In   (params object[] exprs);

        T2 NotIn(params object[] exprs);

        T2 Add(ISqlPredicate predicate);

    }
}