using System.Linq.Expressions;
using LinqToDB.Common;

namespace LinqToDB.Linq.Builder
{
	using LinqToDB.Expressions;
	using LinqToDB.SqlQuery.QueryElements.SqlElements;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Enums;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    class WithTableExpressionBuilder : MethodCallBuilder
	{
		protected override bool CanBuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
		{
			return methodCall.IsQueryable("With", "WithTableExpression");
		}

		protected override IBuildContext BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
		{
			var sequence = builder.BuildSequence(new BuildInfo(buildInfo, methodCall.Arguments[0]));
			var table    = (TableBuilder.TableContext)sequence;
			var value    = (string)((ConstantExpression)methodCall.Arguments[1]).Value;

			table.SqlTable.SqlTableType   = ESqlTableType.Expression;
			table.SqlTable.TableArguments = new IQueryExpression[0];

			switch (methodCall.Method.Name)
			{
				case "With"                : table.SqlTable.Name = "{{0}} {{1}} WITH ({0})".Args(value); break;
				case "WithTableExpression" : table.SqlTable.Name = value;                                break;
			}

			return sequence;
		}

		protected override SequenceConvertInfo Convert(
			ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo, ParameterExpression param)
		{
			return null;
		}
	}
}
