namespace LinqToDB.SqlProvider
{
    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public interface ISqlOptimizer
	{
		SelectQuery    Finalize         (SelectQuery selectQuery);
		ISqlExpression ConvertExpression(ISqlExpression expression);
		ISqlPredicate  ConvertPredicate (SelectQuery selectQuery, ISqlPredicate  predicate);
	}
}
