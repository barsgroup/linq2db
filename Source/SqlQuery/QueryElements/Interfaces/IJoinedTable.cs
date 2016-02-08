namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IJoinedTable: ISqlExpressionWalkable, ICloneableElement, IQueryElement
    {
        EJoinType JoinType { get; set; }

        ITableSource Table { get; set; }

        ISearchCondition Condition { get; }

        bool IsWeak { get; set; }

        bool CanConvertApply { get; set; }
    }
}