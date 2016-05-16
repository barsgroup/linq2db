using System.Linq.Expressions;
using Bars2Db.Common;
using Bars2Db.Expressions;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Enums;

namespace Bars2Db.Linq.Builder
{
    internal class WithTableExpressionBuilder : MethodCallBuilder
    {
        protected override bool CanBuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall,
            BuildInfo buildInfo)
        {
            return methodCall.IsQueryable("With", "WithTableExpression");
        }

        protected override IBuildContext BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall,
            BuildInfo buildInfo)
        {
            var sequence = builder.BuildSequence(new BuildInfo(buildInfo, methodCall.Arguments[0]));
            var table = (TableBuilder.TableContext) sequence;
            var value = (string) ((ConstantExpression) methodCall.Arguments[1]).Value;

            table.SqlTable.SqlTableType = ESqlTableType.Expression;
            table.SqlTable.TableArguments.Clear();

            switch (methodCall.Method.Name)
            {
                case "With":
                    table.SqlTable.Name = "{{0}} {{1}} WITH ({0})".Args(value);
                    break;
                case "WithTableExpression":
                    table.SqlTable.Name = value;
                    break;
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