﻿using System;
using System.Linq.Expressions;

namespace LinqToDB.Linq.Builder
{
	using LinqToDB.Expressions;
	using LinqToDB.SqlQuery.QueryElements.SqlElements;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    class CountBuilder : MethodCallBuilder
	{
		public static readonly string[] MethodNames = { "Count", "LongCount" };

		protected override bool CanBuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
		{
			return methodCall.IsQueryable(MethodNames);
		}

		protected override IBuildContext BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
		{
			var returnType = methodCall.Method.ReturnType;
			var sequence   = builder.BuildSequence(new BuildInfo(buildInfo, methodCall.Arguments[0]) { CreateSubQuery = true });

			if (sequence.Select != buildInfo.SelectQuery)
			{
//				if (sequence is JoinBuilder.GroupJoinSubQueryContext)
//				{
//					var ctx = new CountContext(buildInfo.Parent, sequence, returnType)
//					{
//						SelectQuery =
//							sequence.SelectQuery
//							//((JoinBuilder.GroupJoinSubQueryContext)sequence).GetCounter(methodCall)
//					};
//
//					ctx.Sql        = ctx.SelectQuery;
//					ctx.FieldIndex = ctx.SelectQuery.Select.Add(SqlFunction.CreateCount(returnType, ctx.SelectQuery), "cnt");
//
//					return ctx;
//				}

				if (sequence is GroupByBuilder.GroupByContext)
				{
					return new CountContext(buildInfo.Parent, sequence, returnType)
					{
						Sql        = SqlFunction.CreateCount(returnType, sequence.Select),
						FieldIndex = -1
					};
				}
			}

			if (sequence.Select.Select.IsDistinct        ||
			    sequence.Select.Select.TakeValue != null ||
			    sequence.Select.Select.SkipValue != null)
			{
				sequence.ConvertToIndex(null, 0, ConvertFlags.Key);
				sequence = new SubQueryContext(sequence);
			}
			else if (!sequence.Select.GroupBy.IsEmpty)
			{
				if (!builder.DataContextInfo.SqlProviderFlags.IsSybaseBuggyGroupBy)
					sequence.Select.Select.Add(new SqlValue(0));
				else
					foreach (var item in sequence.Select.GroupBy.Items)
						sequence.Select.Select.Add(item);

				sequence = new SubQueryContext(sequence);
			}

			if (sequence.Select.OrderBy.Items.Count > 0)
			{
				if (sequence.Select.Select.TakeValue == null && sequence.Select.Select.SkipValue == null)
					sequence.Select.OrderBy.Items.Clear();
				else
					sequence = new SubQueryContext(sequence);
			}

			var context = new CountContext(buildInfo.Parent, sequence, returnType);

			context.Sql        = context.Select;
			context.FieldIndex = context.Select.Select.Add(SqlFunction.CreateCount(returnType, context.Select), "cnt");

			return context;
		}

		protected override SequenceConvertInfo Convert(
			ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo, ParameterExpression param)
		{
			return null;
		}

		internal class CountContext : SequenceContextBase
		{
			public CountContext(IBuildContext parent, IBuildContext sequence, Type returnType)
				: base(parent, sequence, null)
			{
				_returnType = returnType;
			}

			readonly Type      _returnType;
			private  SqlInfo[] _index;

			public int            FieldIndex;
			public IQueryExpression Sql;

			public override void BuildQuery<T>(Query<T> query, ParameterExpression queryParameter)
			{
				var expr   = Builder.BuildSql(_returnType, FieldIndex);
				var mapper = Builder.BuildMapper<object>(expr);

				query.SetElementQuery(mapper.Compile());
			}

			public override Expression BuildExpression(Expression expression, int level)
			{
				return Builder.BuildSql(_returnType, ConvertToIndex(expression, level, ConvertFlags.Field)[0].Index);
			}

			public override SqlInfo[] ConvertToSql(Expression expression, int level, ConvertFlags flags)
			{
				switch (flags)
				{
					case ConvertFlags.Field : return new[] { new SqlInfo { Query = Parent.Select, Sql = Sql } };
				}

				throw new NotImplementedException();
			}

			public override SqlInfo[] ConvertToIndex(Expression expression, int level, ConvertFlags flags)
			{
				switch (flags)
				{
					case ConvertFlags.Field :
						return _index ?? (_index = new[]
						{
							new SqlInfo { Query = Parent.Select, Index = Parent.Select.Select.Add(Sql), Sql = Sql, }
						});
				}

				throw new NotImplementedException();
			}

			public override IsExpressionResult IsExpression(Expression expression, int level, RequestFor requestFlag)
			{
				switch (requestFlag)
				{
					case RequestFor.Expression : return IsExpressionResult.True;
				}

				return IsExpressionResult.False;
			}

			public override IBuildContext GetContext(Expression expression, int level, BuildInfo buildInfo)
			{
				return Sequence.GetContext(expression, level, buildInfo);
			}

			public override IQueryExpression GetSubQuery(IBuildContext context)
			{
				var query = context.Select;

				if (query == Select)
				{
					var col = query.Select.Columns[query.Select.Columns.Count - 1];

					query.Select.Columns.RemoveAt(query.Select.Columns.Count - 1);

					return col.Expression;
				}

				return null;
			}
		}
	}
}
