namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IJoin: IConditionBase<IJoin, Join.Next>
    {
        IJoinedTable JoinedTable { get; }

        [SearchContainer]
        // ReSharper disable once UnusedMember.Global
        ISearchCondition Search { get; }

    }
}