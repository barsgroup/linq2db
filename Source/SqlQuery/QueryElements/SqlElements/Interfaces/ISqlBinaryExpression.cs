namespace LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces
{
    public interface ISqlBinaryExpression : IQueryExpression
    {
        IQueryExpression Expr1 { get; set; }

        string Operation { get; }

        IQueryExpression Expr2 { get; set; }
    }
}