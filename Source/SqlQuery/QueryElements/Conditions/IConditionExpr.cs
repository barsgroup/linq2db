namespace LinqToDB.SqlQuery.QueryElements.Conditions
{
    using LinqToDB.SqlQuery.SqlElements;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    interface IConditionExpr<T>
    {
        T Expr    (ISqlExpression expr);
        T Field   (SqlField       field);
        T SubQuery(SelectQuery    selectQuery);
        T Value   (object         value);
    }
}