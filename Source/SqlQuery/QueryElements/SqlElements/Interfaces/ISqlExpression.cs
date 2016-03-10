namespace LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces
{
    using LinqToDB.SqlQuery.Search;

    public interface ISqlExpression : IQueryExpression
    {
        string Expr { get; }

        [SearchContainer]
        IQueryExpression[] Parameters { get; }
    }
}