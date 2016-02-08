namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IConditionExpr<out T>
    {
        T Expr    (ISqlExpression expr);
        T Field   (SqlField       field);
        T SubQuery(ISelectQuery selectQuery);
        T Value   (object         value);
    }
}