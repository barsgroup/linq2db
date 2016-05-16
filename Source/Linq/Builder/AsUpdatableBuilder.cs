using System.Linq.Expressions;
using Bars2Db.Expressions;

namespace Bars2Db.Linq.Builder
{
    internal class AsUpdatableBuilder : MethodCallBuilder
    {
        protected override bool CanBuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall,
            BuildInfo buildInfo)
        {
            return methodCall.IsQueryable("AsUpdatable");
        }

        protected override IBuildContext BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall,
            BuildInfo buildInfo)
        {
            return builder.BuildSequence(new BuildInfo(buildInfo, methodCall.Arguments[0]));
        }

        protected override SequenceConvertInfo Convert(
            ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo, ParameterExpression param)
        {
            return null;
        }
    }
}