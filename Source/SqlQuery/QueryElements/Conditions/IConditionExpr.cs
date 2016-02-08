namespace LinqToDB.SqlQuery.QueryElements.Conditions
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    interface IConditionExpr<out T>
    {
        T Expr    (ISqlExpression expr);
        T Field   (SqlField       field);
        T SubQuery(ISelectQuery selectQuery);
        T Value   (object         value);
    }
}