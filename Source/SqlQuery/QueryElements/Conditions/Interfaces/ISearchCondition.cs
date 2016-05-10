using System.Collections.Generic;
using Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces
{
    public interface ISearchCondition : IQueryExpression,
        IConditionBase<ISearchCondition, SearchCondition.NextCondition>, ISqlPredicate
    {
        [SearchContainer]
        LinkedList<ICondition> Conditions { get; }

        ISearchCondition Search { get; }
    }
}