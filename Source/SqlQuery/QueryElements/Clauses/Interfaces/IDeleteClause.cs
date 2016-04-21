namespace LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IDeleteClause : ISqlExpressionWalkable,
                                     ICloneableElement, IQueryElement
    {
        [SearchContainer]
        ISqlTable Table { get; set; }
    }
}