namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IInSubQuery: INotExpr
    {
        [SearchContainer]
        ISelectQuery SubQuery { get; }
    }
}