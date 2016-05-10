using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces
{
    public interface IJoin : IConditionBase<IJoin, Join.Next>
    {
        IJoinedTable JoinedTable { get; }

        [SearchContainer]
        // ReSharper disable once UnusedMember.Global
        ISearchCondition Search { get; }
    }
}