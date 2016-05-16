using System.Linq.Expressions;
using Bars2Db.Expressions;

namespace Bars2Db.Linq.Builder
{
    internal class ChangeTypeExpressionBuilder : ISequenceBuilder
    {
        private ISequenceBuilder _builder;
        public int BuildCounter { get; set; }

        public bool CanBuild(ExpressionBuilder builder, BuildInfo buildInfo)
        {
            return buildInfo.Expression is ChangeTypeExpression;
        }

        public IBuildContext BuildSequence(ExpressionBuilder builder, BuildInfo buildInfo)
        {
            var expr = (ChangeTypeExpression) buildInfo.Expression;
            var info = new BuildInfo(buildInfo, expr.Expression);

            return GetBuilder(builder, info).BuildSequence(builder, info);
        }

        public SequenceConvertInfo Convert(ExpressionBuilder builder, BuildInfo buildInfo, ParameterExpression param)
        {
            return null;
        }

        public bool IsSequence(ExpressionBuilder builder, BuildInfo buildInfo)
        {
            var expr = (ChangeTypeExpression) buildInfo.Expression;
            var info = new BuildInfo(buildInfo, expr.Expression);

            return GetBuilder(builder, info).IsSequence(builder, info);
        }

        private ISequenceBuilder GetBuilder(ExpressionBuilder builder, BuildInfo buildInfo)
        {
            return _builder ?? (_builder = builder.GetBuilder(buildInfo));
        }
    }
}