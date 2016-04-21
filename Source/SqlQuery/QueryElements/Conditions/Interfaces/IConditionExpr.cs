namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IConditionExpr<out T>
    {
        T Expr    (IQueryExpression expr);
        T Field   (ISqlField field);
    }
}