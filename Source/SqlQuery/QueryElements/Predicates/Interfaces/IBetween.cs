namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IBetween: INotExpr
    {
        [SearchContainer]
        IQueryExpression Expr2 { get; set; }

        [SearchContainer]
        IQueryExpression Expr3 { get; set; }
    }
}