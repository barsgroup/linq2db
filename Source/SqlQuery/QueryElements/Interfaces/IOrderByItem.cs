namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IOrderByItem: IQueryElement, ISqlExpressionWalkable, ICloneableElement
    {
        IQueryExpression Expression { get; set; }

        bool IsDescending { get; }
    }
}