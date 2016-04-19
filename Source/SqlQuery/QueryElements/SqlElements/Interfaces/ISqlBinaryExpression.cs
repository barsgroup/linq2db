namespace LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces
{
    using LinqToDB.SqlQuery.Search;

    public interface ISqlBinaryExpression : IQueryExpression
    {
        [SearchContainer]
        IQueryExpression Expr1 { get; set; }

        string Operation { get; }

        [SearchContainer]
        IQueryExpression Expr2 { get; set; }
    }
}