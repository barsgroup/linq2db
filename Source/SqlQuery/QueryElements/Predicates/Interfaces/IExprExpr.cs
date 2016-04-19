namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IExprExpr: IExpr
    {
        EOperator EOperator { get; }

        [SearchContainer]
        IQueryExpression Expr2 { get; set; }
    }
}