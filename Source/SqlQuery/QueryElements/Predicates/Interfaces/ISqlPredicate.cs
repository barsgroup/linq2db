namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ISqlPredicate : IQueryElement, ISqlExpressionWalkable, ICloneableElement,  IOperation
    {
    }
}
