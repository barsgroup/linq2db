using System.Linq.Expressions;

#if DEBUG
// ReSharper disable InconsistentNaming

#pragma warning disable 3010
#endif

namespace LinqToDB.Linq.Builder
{
    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public interface IBuildContext
	{
#if DEBUG
		string _sqlQueryText { get; }
#endif

		ExpressionBuilder  Builder     { get; }
		Expression         Expression  { get; }
		SelectQuery        SelectQuery { get; set; }
		IBuildContext      Parent      { get; set; }

		void               BuildQuery<T>       (Query<T> query, ParameterExpression queryParameter);
		Expression         BuildExpression     (Expression expression, int level);
		SqlInfo[]          ConvertToSql        (Expression expression, int level, ConvertFlags flags);
		SqlInfo[]          ConvertToIndex      (Expression expression, int level, ConvertFlags flags);
		IsExpressionResult IsExpression        (Expression expression, int level, RequestFor requestFlag);
		IBuildContext      GetContext          (Expression expression, int level, BuildInfo buildInfo);
		int                ConvertToParentIndex(int index, IBuildContext context);
		void               SetAlias            (string alias);
		ISqlExpression     GetSubQuery         (IBuildContext context);
	}
}
