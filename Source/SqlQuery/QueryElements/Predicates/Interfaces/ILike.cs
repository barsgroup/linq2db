namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface ILike: INotExpr
    {
        [SearchContainer]
        IQueryExpression Expr2 { get; set; }

        [SearchContainer]
        IQueryExpression Escape { get; set; }

        string GetOperator();
    }
}