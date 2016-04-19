namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface ISetExpression: ISqlExpressionWalkable, ICloneableElement, IQueryElement
    {
        [SearchContainer]
        IQueryExpression Column { get; set; }

        [SearchContainer]
        IQueryExpression Expression { get; set; }
    }
}