using System.Linq.Expressions;
using Bars2Db.Expressions;

namespace Bars2Db.Linq.Builder
{
    internal class DistinctBuilder : MethodCallBuilder
    {
        protected override bool CanBuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall,
            BuildInfo buildInfo)
        {
            return methodCall.IsQueryable("Distinct");
        }

        protected override IBuildContext BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall,
            BuildInfo buildInfo)
        {
            var sequence = builder.BuildSequence(new BuildInfo(buildInfo, methodCall.Arguments[0]));
            var sql = sequence.Select;

            if (sql.Select.TakeValue != null || sql.Select.SkipValue != null)
                sequence = new SubQueryContext(sequence);

            sequence.Select.Select.IsDistinct = true;
            sequence.ConvertToIndex(null, 0, ConvertFlags.All);

            return sequence;
        }

        protected override SequenceConvertInfo Convert(
            ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo, ParameterExpression param)
        {
            return null;
        }
    }
}