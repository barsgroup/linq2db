using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqToDB.Linq.Builder
{
	using Extensions;
	using LinqToDB.Expressions;
	using LinqToDB.SqlQuery.QueryElements;
	using LinqToDB.SqlQuery.QueryElements.Enums;
	using LinqToDB.SqlQuery.QueryElements.Interfaces;
	using LinqToDB.SqlQuery.QueryElements.SqlElements;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

	using SqlQuery;

	class UpdateBuilder : MethodCallBuilder
	{
		#region Update

		protected override bool CanBuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
		{
			return methodCall.IsQueryable("Update");
		}

		protected override IBuildContext BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
		{
			var sequence = builder.BuildSequence(new BuildInfo(buildInfo, methodCall.Arguments[0]));

			switch (methodCall.Arguments.Count)
			{
				case 1 : // int Update<T>(this IUpdateable<T> source)
					CheckAssociation(sequence);
					break;

				case 2 : // int Update<T>(this IQueryable<T> source, Expression<Func<T,T>> setter)
					{
						CheckAssociation(sequence);

						BuildSetter(
							builder,
							buildInfo,
							(LambdaExpression)methodCall.Arguments[1].Unwrap(),
							sequence,
							sequence.Select.Update.Items,
							sequence);
						break;
					}

				case 3 :
					{
						var expr = methodCall.Arguments[1].Unwrap();

						if (expr is LambdaExpression)
						{
							CheckAssociation(sequence);

							// int Update<T>(this IQueryable<T> source, Expression<Func<T,bool>> predicate, Expression<Func<T,T>> setter)
							//
							sequence = builder.BuildWhere(buildInfo.Parent, sequence, (LambdaExpression)methodCall.Arguments[1].Unwrap(), false);

							BuildSetter(
								builder,
								buildInfo,
								(LambdaExpression)methodCall.Arguments[2].Unwrap(),
								sequence,
								sequence.Select.Update.Items,
								sequence);
						}
						else
						{
							// static int Update<TSource,TTarget>(this IQueryable<TSource> source, Table<TTarget> target, Expression<Func<TSource,TTarget>> setter)
							//
							var into = builder.BuildSequence(new BuildInfo(buildInfo, expr, new SelectQuery()));

							sequence.ConvertToIndex(null, 0, ConvertFlags.All);
							new SelectQueryOptimizer(builder.DataContextInfo.SqlProviderFlags, sequence.Select)
								.ResolveWeakJoins(new List<ISqlTableSource>());
							sequence.Select.Select.Columns.Clear();

							BuildSetter(
								builder,
								buildInfo,
								(LambdaExpression)methodCall.Arguments[2].Unwrap(),
								into,
								sequence.Select.Update.Items,
								sequence);

							var sql = sequence.Select;

							sql.Select.Columns.Clear();

							foreach (var item in sql.Update.Items)
								sql.Select.Columns.Add(new Column(sql, item.Expression));

							sql.Update.Table = ((TableBuilder.TableContext)into).SqlTable;
						}

						break;
					}
			}

			sequence.Select.EQueryType = EQueryType.Update;

			return new UpdateContext(buildInfo.Parent, sequence);
		}

		static void CheckAssociation(IBuildContext sequence)
		{
			var ctx = sequence as SelectContext;
		    if (ctx == null || !ctx.IsScalar)
		    {
		        return;
		    }

		    var res = ctx.IsExpression(null, 0, RequestFor.Association);

		    var associatedTableContext = res.Context as TableBuilder.AssociatedTableContext;
		    if (res.Result && associatedTableContext != null)
		    {
		        sequence.Select.Update.Table = associatedTableContext.SqlTable;
		    }
		    else
		    {
		        res = ctx.IsExpression(null, 0, RequestFor.Table);

		        var tableContext = res.Context as TableBuilder.TableContext;
		        if (res.Result && tableContext != null)
		        {
		            if (sequence.Select.From.Tables.Count == 0 || sequence.Select.From.Tables.First.Value.Source != tableContext.Select)
		                sequence.Select.Update.Table = tableContext.SqlTable;
		        }
		    }
		}

		protected override SequenceConvertInfo Convert(
			ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo, ParameterExpression param)
		{
			return null;
		}

		#endregion

		#region Helpers

		internal static void BuildSetter(
			ExpressionBuilder               builder,
			BuildInfo                       buildInfo,
			LambdaExpression                setter,
			IBuildContext                   into,
			LinkedList<ISetExpression> items,
			IBuildContext                   sequence)
		{
			var path = Expression.Parameter(setter.Body.Type, "p");
			var ctx  = new ExpressionContext(buildInfo.Parent, sequence, setter);

			if (setter.Body.NodeType == ExpressionType.MemberInit)
			{
				var ex  = (MemberInitExpression)setter.Body;
				var p   = sequence.Parent;

				BuildSetter(builder, into, items, ctx, ex, path);

				builder.ReplaceParent(ctx, p);
			}
			else
			{
				var sqlInfo = ctx.ConvertToSql(setter.Body, 0, ConvertFlags.All);

				foreach (var info in sqlInfo)
				{
					if (info.Members.Count == 0)
						throw new LinqException("Object initializer expected for insert statement.");

					if (info.Members.Count != 1)
						throw new InvalidOperationException();

					var member = info.Members[0];
					var pe     = Expression.MakeMemberAccess(path, member);
					var column = into.ConvertToSql(pe, 1, ConvertFlags.Field);
					var expr   = info.Sql;

					items.AddLast(new SetExpression(column[0].Sql, expr));
				}
			}
		}

		static void BuildSetter(
			ExpressionBuilder               builder,
			IBuildContext                   into,
			LinkedList<ISetExpression> items,
			IBuildContext                   ctx,
			MemberInitExpression            expression,
			Expression                      path)
		{
			foreach (var binding in expression.Bindings)
			{
				var member  = binding.Member;

			    var methodInfo = member as MethodInfo;
			    if (methodInfo != null)
					member = methodInfo.GetPropertyInfo();

			    var memberAssignment = binding as MemberAssignment;
			    if (memberAssignment != null)
				{
				    var pe = Expression.MakeMemberAccess(path, member);

				    var initExpression = memberAssignment.Expression as MemberInitExpression;
				    if (initExpression != null && !into.IsExpression(pe, 1, RequestFor.Field).Result)
					{
						BuildSetter(
							builder,
							into,
							items,
							ctx,
							initExpression, Expression.MakeMemberAccess(path, member));
					}
					else
					{
						var column = into.ConvertToSql(pe, 1, ConvertFlags.Field);
						var expr   = builder.ConvertToSqlExpression(ctx, memberAssignment.Expression);

						if (expr.ElementType == EQueryElementType.SqlParameter)
						{
							var parm  = (ISqlParameter)expr;
							var field = (ISqlField)column[0].Sql;

							if (parm.DataType == DataType.Undefined)
								parm.DataType = field.DataType;
						}

						items.AddLast(new SetExpression(column[0].Sql, expr));
					}
				}
				else
					throw new InvalidOperationException();
			}
		}

		internal static void ParseSet(
			ExpressionBuilder               builder,
			BuildInfo                       buildInfo,
			LambdaExpression                extract,
			LambdaExpression                update,
			IBuildContext                   select,
            ISqlTable                        table,
			LinkedList<ISetExpression> items)
		{
			var ext = extract.Body;

			while (ext.NodeType == ExpressionType.Convert || ext.NodeType == ExpressionType.ConvertChecked)
				ext = ((UnaryExpression)ext).Operand;

			if (ext.NodeType != ExpressionType.MemberAccess || ext.GetRootObject() != extract.Parameters[0])
				throw new LinqException("Member expression expected for the 'Set' statement.");

			var body   = (MemberExpression)ext;
			var member = body.Member;

		    var info = member as MethodInfo;
		    if (info != null)
				member = info.GetPropertyInfo();

			var members = body.GetMembers();
			var name    = members
				.Skip(1)
				.Select(ex =>
				{
					var me = ex as MemberExpression;
					if (me == null)
						return null;

					var m = me.Member;

				    var methodInfo = m as MethodInfo;
				    if (methodInfo != null)
						m = methodInfo.GetPropertyInfo();

					return m;
				})
				.Where(m => m != null && !m.IsNullableValueMember())
				.Select(m => m.Name)
				.Aggregate((s1,s2) => s1 + "." + s2);

			if (table != null && !table.Fields.ContainsKey(name))
				throw new LinqException("Member '{0}.{1}' is not a table column.", member.DeclaringType.Name, name);

			var column = table != null ?
				table.Fields[name] :
				select.ConvertToSql(
					body, 1, ConvertFlags.Field)[0].Sql;
					//Expression.MakeMemberAccess(Expression.Parameter(member.DeclaringType, "p"), member), 1, ConvertFlags.Field)[0].Sql;
			var sp     = select.Parent;
			var ctx    = new ExpressionContext(buildInfo.Parent, select, update);
			var expr   = builder.ConvertToSqlExpression(ctx, update.Body);

			builder.ReplaceParent(ctx, sp);

			items.AddLast(new SetExpression(column, expr));
		}

		internal static void ParseSet(
			ExpressionBuilder               builder,
			BuildInfo                       buildInfo,
			LambdaExpression                extract,
			Expression                      update,
			IBuildContext                   select,
			LinkedList<ISetExpression> items)
		{
			var ext = extract.Body;

			if (!update.Type.IsConstantable() && !builder.AsParameters.Contains(update))
				builder.AsParameters.Add(update);

			while (ext.NodeType == ExpressionType.Convert || ext.NodeType == ExpressionType.ConvertChecked)
				ext = ((UnaryExpression)ext).Operand;

			if (ext.NodeType != ExpressionType.MemberAccess || ext.GetRootObject() != extract.Parameters[0])
				throw new LinqException("Member expression expected for the 'Set' statement.");

			var body   = (MemberExpression)ext;
			var member = body.Member;

		    var methodInfo = member as MethodInfo;
		    if (methodInfo != null)
				member = methodInfo.GetPropertyInfo();

			var column = select.ConvertToSql(body, 1, ConvertFlags.Field);

			if (column.Length == 0)
				throw new LinqException("Member '{0}.{1}' is not a table column.", member.DeclaringType.Name, member.Name);

			var expr = builder.ConvertToSql(select, update);

			items.AddLast(new SetExpression(column[0].Sql, expr));
		}

		#endregion

		#region UpdateContext

		class UpdateContext : SequenceContextBase
		{
			public UpdateContext(IBuildContext parent, IBuildContext sequence)
				: base(parent, sequence, null)
			{
			}

			public override void BuildQuery<T>(Query<T> query, ParameterExpression queryParameter)
			{
				query.SetNonQueryQuery();
			}

			public override Expression BuildExpression(Expression expression, int level)
			{
				throw new NotImplementedException();
			}

			public override SqlInfo[] ConvertToSql(Expression expression, int level, ConvertFlags flags)
			{
				throw new NotImplementedException();
			}

			public override SqlInfo[] ConvertToIndex(Expression expression, int level, ConvertFlags flags)
			{
				throw new NotImplementedException();
			}

			public override IsExpressionResult IsExpression(Expression expression, int level, RequestFor requestFlag)
			{
				throw new NotImplementedException();
			}

			public override IBuildContext GetContext(Expression expression, int level, BuildInfo buildInfo)
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region Set

		internal class Set : MethodCallBuilder
		{
			protected override bool CanBuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
			{
				return methodCall.IsQueryable("Set");
			}

			protected override IBuildContext BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
			{
				var sequence = builder.BuildSequence(new BuildInfo(buildInfo, methodCall.Arguments[0]));
				var extract  = (LambdaExpression)methodCall.Arguments[1].Unwrap();
				var update   =                   methodCall.Arguments[2].Unwrap();

				if (update.NodeType == ExpressionType.Lambda)
					ParseSet(
						builder,
						buildInfo,
						extract,
						(LambdaExpression)update,
						sequence,
						sequence.Select.Update.Table,
						sequence.Select.Update.Items);
				else
					ParseSet(
						builder,
						buildInfo,
						extract,
						update,
						sequence,
						sequence.Select.Update.Items);

				return sequence;
			}

			protected override SequenceConvertInfo Convert(
				ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo, ParameterExpression param)
			{
				return null;
			}
		}

		#endregion
	}
}
