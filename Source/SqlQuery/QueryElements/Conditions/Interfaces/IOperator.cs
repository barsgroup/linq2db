namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public interface IOperator<out T2> : IConditionExpr<T2>
    {
        T2 All     (ISelectQuery subQuery);

        T2 Some    (ISelectQuery subQuery);

        T2 Any     (ISelectQuery subQuery);
    }
}