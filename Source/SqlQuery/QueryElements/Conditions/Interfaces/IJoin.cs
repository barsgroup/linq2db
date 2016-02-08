namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public interface IJoin: IConditionBase<IJoin, Join.Next>
    {
        IJoinedTable JoinedTable { get; }
    }
}