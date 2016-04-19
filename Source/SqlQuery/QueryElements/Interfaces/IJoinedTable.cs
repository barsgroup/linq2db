namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IJoinedTable: ISqlExpressionWalkable, ICloneableElement, IQueryElement
    {
        EJoinType JoinType { get; set; }

        [SearchContainer]
        ITableSource Table { get; set; }

        [SearchContainer]
        ISearchCondition Condition { get; }

        bool IsWeak { get; set; }

        bool CanConvertApply { get; set; }
    }
}