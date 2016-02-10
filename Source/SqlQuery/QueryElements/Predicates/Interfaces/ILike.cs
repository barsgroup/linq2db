namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ILike: INotExpr
    {
        IQueryExpression Expr2 { get; set; }

        IQueryExpression Escape { get; set; }

        string GetOperator();
    }
}