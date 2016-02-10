namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ISetExpression: ISqlExpressionWalkable, ICloneableElement, IQueryElement
    {
        IQueryExpression Column { get; set; }

        IQueryExpression Expression { get; set; }
    }
}