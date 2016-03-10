namespace LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces
{
    using LinqToDB.SqlQuery.Search;

    public interface ISqlFunction : IQueryExpression
    {
        string Name { get; }

        [SearchContainer]
        IQueryExpression[] Parameters { get; }
    }
}