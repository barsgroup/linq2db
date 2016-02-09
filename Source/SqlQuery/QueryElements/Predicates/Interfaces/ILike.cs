namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ILike: INotExpr
    {
        ISqlExpression Expr2 { get; set; }

        ISqlExpression Escape { get; set; }

        string GetOperator();
    }
}