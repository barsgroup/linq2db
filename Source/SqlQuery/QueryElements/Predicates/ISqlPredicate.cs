namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using SqlElements.Interfaces;
    public interface ISqlPredicate : IQueryElement, ISqlExpressionWalkable, ICloneableElement
	{
		bool CanBeNull();
		int  Precedence { get; }
	}
}
