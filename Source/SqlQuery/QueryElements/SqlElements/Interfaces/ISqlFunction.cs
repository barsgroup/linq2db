namespace LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces
{
    public interface ISqlFunction : IQueryExpression
    {
        string Name { get; }

        IQueryExpression[] Parameters { get; }
    }
}