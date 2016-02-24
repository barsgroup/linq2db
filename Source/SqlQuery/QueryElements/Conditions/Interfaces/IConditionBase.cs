namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public interface IConditionBase<T1, out T2> : IConditionExpr<IExpr<T1, T2>>, IQueryElement,
                                                  IHaveSearchCondition
        where T1 : IConditionBase<T1, T2>
    {
        T2 GetNext();

        INot<T1, T2> Not { get; }

        T1 SetOr(bool value);

        T2 Exists(ISelectQuery subQuery);

    }
}