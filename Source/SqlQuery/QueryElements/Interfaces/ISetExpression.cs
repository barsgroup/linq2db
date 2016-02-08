namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ISetExpression: ISqlExpressionWalkable, ICloneableElement, IQueryElement
    {
        ISqlExpression Column { get; set; }

        ISqlExpression Expression { get; set; }
    }
}