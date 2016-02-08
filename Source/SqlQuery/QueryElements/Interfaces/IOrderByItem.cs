namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IOrderByItem: IQueryElement, ISqlExpressionWalkable, ICloneableElement
    {
        ISqlExpression Expression { get; set; }

        bool IsDescending { get; }
    }
}