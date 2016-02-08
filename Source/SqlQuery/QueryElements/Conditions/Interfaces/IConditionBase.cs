namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public interface IConditionBase<T1, T2> : IConditionExpr<IExpr<T1, T2>>, IQueryElement
        where T1 : IConditionBase<T1, T2>
    {
        T2 GetNext();

        ISearchCondition Search { get; }

        INot<T1, T2> Not { get; }

        T1 SetOr(bool value);

        T2 Exists(ISelectQuery subQuery);

    }
}