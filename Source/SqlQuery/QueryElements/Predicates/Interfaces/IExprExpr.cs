namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IExprExpr: IExpr
    {
        EOperator EOperator { get; }

        IQueryExpression Expr2 { get; set; }
    }
}