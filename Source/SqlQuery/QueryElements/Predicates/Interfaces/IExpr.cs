namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IExpr: ISqlPredicate
    {
        [SearchContainer]
        IQueryExpression Expr1 { get; set; }
    }
}