namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public interface INot<T1, out T2> : IConditionExpr<IExpr<T1, T2>>
        where T1 : IConditionBase<T1, T2>
    {
        T2 Exists(ISelectQuery subQuery);
    }
}