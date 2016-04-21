namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface ICreateTableStatement : IQueryElement,
                                             ISqlExpressionWalkable,
                                             ICloneableElement
    {
        [SearchContainer]
        ISqlTable Table { get; set; }

        bool IsDrop { get; set; }

        string StatementHeader { get; set; }

        string StatementFooter { get; set; }

        EDefaulNullable EDefaulNullable { get; set; }
    }
}