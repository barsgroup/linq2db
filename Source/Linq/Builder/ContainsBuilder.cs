using System;
using System.Linq.Expressions;

namespace LinqToDB.Linq.Builder
{
    using LinqToDB.Expressions;
    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using SqlQuery.QueryElements.Conditions.Interfaces;
    class ContainsBuilder : MethodCallBuilder
	{
		protected override bool CanBuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
		{
			return methodCall.IsQueryable("Contains") && methodCall.Arguments.Count == 2;
		}

		protected override IBuildContext BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
		{
			var sequence = builder.BuildSequence(new BuildInfo(buildInfo, methodCall.Arguments[0]));
			return new ContainsContext(buildInfo.Parent, methodCall, sequence);
		}

		protected override SequenceConvertInfo Convert(
			ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo, ParameterExpression param)
		{
			return null;
		}

		class ContainsContext : SequenceContextBase
		{
			readonly MethodCallExpression _methodCall;

			public ContainsContext(IBuildContext parent, MethodCallExpression methodCall, IBuildContext sequence)
				: base(parent, sequence, null)
			{
				_methodCall = methodCall;
			}

			public override void BuildQuery<T>(Query<T> query, ParameterExpression queryParameter)
			{
				var sql = GetSubQuery(null);

				query.Queries[0].SelectQuery = new SelectQuery();
				query.Queries[0].SelectQuery.Select.Add(sql);

				var expr   = Builder.BuildSql(typeof(bool), 0);
				var mapper = Builder.BuildMapper<object>(expr);

				query.SetElementQuery(mapper.Compile());
			}

			public override Expression BuildExpression(Expression expression, int level)
			{
				var idx = ConvertToIndex(expression, level, ConvertFlags.Field);
				return Builder.BuildSql(typeof(bool), idx[0].Index);
			}

			public override SqlInfo[] ConvertToSql(Expression expression, int level, ConvertFlags flags)
			{
				if (expression == null)
				{
					var sql   = GetSubQuery(null);
					var query = Select;

					if (Parent != null)
						query = Parent.Select;

					return new[] { new SqlInfo { Query = query, Sql = sql } };
				}

				throw new InvalidOperationException();
			}

			public override SqlInfo[] ConvertToIndex(Expression expression, int level, ConvertFlags flags)
			{
				var sql = ConvertToSql(expression, level, flags);

				if (sql[0].Index < 0)
					sql[0].Index = sql[0].Query.Select.Add(sql[0].Sql);

				return sql;
			}

			public override IsExpressionResult IsExpression(Expression expression, int level, RequestFor requestFlag)
			{
				if (expression == null)
				{
					switch (requestFlag)
					{
						case RequestFor.Expression :
						case RequestFor.Field      : return IsExpressionResult.False;
					}
				}

				switch (requestFlag)
				{
					case RequestFor.Root : return IsExpressionResult.False;
				}

				throw new InvalidOperationException();
			}

			public override IBuildContext GetContext(Expression expression, int level, BuildInfo buildInfo)
			{
				throw new InvalidOperationException();
			}

			IQueryExpression _subQuerySql;

			public override IQueryExpression GetSubQuery(IBuildContext context)
			{
				if (_subQuerySql == null)
				{
					var args      = _methodCall.Method.GetGenericArguments();
					var param     = Expression.Parameter(args[0], "param");
					var expr      = _methodCall.Arguments[1];
					var condition = Expression.Lambda(Expression.Equal(param, expr), param);

					IBuildContext ctx = new ExpressionContext(Parent, Sequence, condition);

					ctx = Builder.GetContext(ctx, expr) ?? ctx;

					Builder.ReplaceParent(ctx, this);

                    ICondition cond;

					if (Sequence.Select != Select &&
						(ctx.IsExpression(expr, 0, RequestFor.Field).     Result ||
						 ctx.IsExpression(expr, 0, RequestFor.Expression).Result))
					{
						Sequence.ConvertToIndex(null, 0, ConvertFlags.All);
						var ex = Builder.ConvertToSql(ctx, _methodCall.Arguments[1]);
						cond = new Condition(false, new InSubQuery(ex, false, Select));
					}
					else
					{
						var sequence = Builder.BuildWhere(Parent, Sequence, condition, true);
						cond = new Condition(false, new FuncLike(SqlFunction.CreateExists(sequence.Select)));
					}

					_subQuerySql = new SearchCondition(cond);
				}

				return _subQuerySql;
			}
		}
	}
}
