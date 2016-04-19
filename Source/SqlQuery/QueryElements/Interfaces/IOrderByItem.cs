namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IOrderByItem: IQueryElement, ISqlExpressionWalkable, ICloneableElement
    {
        [SearchContainer]
        IQueryExpression Expression { get; set; }

        bool IsDescending { get; }
    }
}