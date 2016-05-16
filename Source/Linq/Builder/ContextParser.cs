using System;
using System.Linq.Expressions;
using Bars2Db.SqlProvider;

namespace Bars2Db.Linq.Builder
{
    internal class ContextParser : ISequenceBuilder
    {
        public int BuildCounter { get; set; }

        public bool CanBuild(ExpressionBuilder builder, BuildInfo buildInfo)
        {
            var call = buildInfo.Expression as MethodCallExpression;
            return call != null && call.Method.Name == "GetContext";
        }

        public IBuildContext BuildSequence(ExpressionBuilder builder, BuildInfo buildInfo)
        {
            var call = (MethodCallExpression) buildInfo.Expression;
            return new Context(builder.BuildSequence(new BuildInfo(buildInfo, call.Arguments[0])));
        }

        public SequenceConvertInfo Convert(ExpressionBuilder builder, BuildInfo buildInfo, ParameterExpression param)
        {
            return null;
        }

        public bool IsSequence(ExpressionBuilder builder, BuildInfo buildInfo)
        {
            return
                builder.IsSequence(new BuildInfo(buildInfo, ((MethodCallExpression) buildInfo.Expression).Arguments[0]));
        }

        public class Context : PassThroughContext
        {
            public Action SetParameters;

            public ISqlOptimizer SqlOptimizer;

            public Context(IBuildContext context) : base(context)
            {
            }

            public override void BuildQuery<T>(Query<T> query, ParameterExpression queryParameter)
            {
                query.SetNonQueryQuery();

                SqlOptimizer = query.SqlOptimizer;
                SetParameters = () => query.SetParameters(Builder.Expression, null, 0);

                query.GetElement = (ctx, db, expr, ps) => this;
            }
        }
    }
}