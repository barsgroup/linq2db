namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public interface IInSubQuery: INotExpr
    {
        ISelectQuery SubQuery { get; }
    }
}