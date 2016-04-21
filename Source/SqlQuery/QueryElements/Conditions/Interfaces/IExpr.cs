namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IExpr<T1, out T2>
        where T1 : IConditionBase<T1, T2>
    {
        IOperator<T2> Equal { get; }

        IOperator<T2> NotEqual { get; }

        IOperator<T2> Greater { get; }

        IQueryExpression SqlExpression { get; }

        T2 IsNull { get; }

        T2 IsNotNull { get; }

        T2 Add(ISqlPredicate predicate);

    }
}