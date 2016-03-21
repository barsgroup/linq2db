namespace LinqToDB.SqlProvider
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ISqlOptimizer
    {
        ISelectQuery Finalize         (ISelectQuery selectQuery);
        IQueryExpression ConvertExpression(IQueryExpression expression);
        ISqlPredicate  ConvertPredicate (ISelectQuery selectQuery, ISqlPredicate  predicate);
    }
}
