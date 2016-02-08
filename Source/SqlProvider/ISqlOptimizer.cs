namespace LinqToDB.SqlProvider
{
    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ISqlOptimizer
	{
        ISelectQuery Finalize         (ISelectQuery selectQuery);
		ISqlExpression ConvertExpression(ISqlExpression expression);
		ISqlPredicate  ConvertPredicate (ISelectQuery selectQuery, ISqlPredicate  predicate);
	}
}
