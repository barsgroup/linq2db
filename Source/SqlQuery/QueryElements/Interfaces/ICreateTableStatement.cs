namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ICreateTableStatement : IQueryElement,
                                             ISqlExpressionWalkable,
                                             ICloneableElement
    {
        ISqlTable Table { get; set; }

        bool IsDrop { get; set; }

        string StatementHeader { get; set; }

        string StatementFooter { get; set; }

        EDefaulNullable EDefaulNullable { get; set; }
    }
}