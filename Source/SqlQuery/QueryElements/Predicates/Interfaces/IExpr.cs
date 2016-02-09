namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IExpr: ISqlPredicate
    {
        ISqlExpression Expr1 { get; set; }
    }
}