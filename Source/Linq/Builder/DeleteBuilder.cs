﻿using System;
using System.Linq.Expressions;
using Bars2Db.Expressions;
using Bars2Db.SqlQuery.QueryElements.Enums;

namespace Bars2Db.Linq.Builder
{
    internal class DeleteBuilder : MethodCallBuilder
    {
        protected override bool CanBuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall,
            BuildInfo buildInfo)
        {
            return methodCall.IsQueryable("Delete");
        }

        protected override IBuildContext BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall,
            BuildInfo buildInfo)
        {
            var sequence = builder.BuildSequence(new BuildInfo(buildInfo, methodCall.Arguments[0]));

            if (methodCall.Arguments.Count == 2)
                sequence = builder.BuildWhere(buildInfo.Parent, sequence,
                    (LambdaExpression) methodCall.Arguments[1].Unwrap(), false);

            sequence.Select.EQueryType = EQueryType.Delete;

            // Check association.
            //
            var ctx = sequence as SelectContext;
            if (ctx != null && ctx.IsScalar)
            {
                var res = ctx.IsExpression(null, 0, RequestFor.Association);

                var associatedTableContext = res.Context as TableBuilder.AssociatedTableContext;
                if (res.Result && associatedTableContext != null)
                {
                    sequence.Select.Delete.Table = associatedTableContext.SqlTable;
                }
                else
                {
                    res = ctx.IsExpression(null, 0, RequestFor.Table);

                    var tableContext = res.Context as TableBuilder.TableContext;
                    if (res.Result && tableContext != null)
                    {
                        if (sequence.Select.From.Tables.Count == 0 ||
                            sequence.Select.From.Tables.First.Value.Source != tableContext.Select)
                            sequence.Select.Delete.Table = tableContext.SqlTable;
                    }
                }
            }

            return new DeleteContext(buildInfo.Parent, sequence);
        }

        protected override SequenceConvertInfo Convert(
            ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo, ParameterExpression param)
        {
            return null;
        }

        private class DeleteContext : SequenceContextBase
        {
            public DeleteContext(IBuildContext parent, IBuildContext sequence)
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
    }
}