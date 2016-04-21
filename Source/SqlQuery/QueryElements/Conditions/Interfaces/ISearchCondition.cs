namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface ISearchCondition : IQueryExpression,
                                        IConditionBase<ISearchCondition, SearchCondition.NextCondition>, ISqlPredicate
    {
        [SearchContainer]
        LinkedList<ICondition> Conditions { get; }

        ISearchCondition Search { get; }
    }
}