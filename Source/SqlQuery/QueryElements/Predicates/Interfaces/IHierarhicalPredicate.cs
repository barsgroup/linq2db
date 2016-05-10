namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IHierarhicalPredicate: IExpr
    {
        [SearchContainer]
        IQueryExpression Expr2 { get; set; }

        HierarhicalFlow Flow { get; }

        string GetOperator();
    }
}