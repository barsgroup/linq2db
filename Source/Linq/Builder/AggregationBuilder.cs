﻿using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Common;
using Bars2Db.Expressions;
using Bars2Db.Extensions;
using Bars2Db.SqlQuery.QueryElements;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.Linq.Builder
{
    internal class AggregationBuilder : MethodCallBuilder
    {
        public static string[] MethodNames = {"Average", "Min", "Max", "Sum"};

        protected override bool CanBuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall,
            BuildInfo buildInfo)
        {
            return methodCall.IsQueryable(MethodNames);
        }

        protected override IBuildContext BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall,
            BuildInfo buildInfo)
        {
            var sequence = builder.BuildSequence(new BuildInfo(buildInfo, methodCall.Arguments[0]));

            if (sequence.Select.Select.IsDistinct ||
                sequence.Select.Select.TakeValue != null ||
                sequence.Select.Select.SkipValue != null ||
                !sequence.Select.GroupBy.IsEmpty)
            {
                sequence = new SubQueryContext(sequence);
            }

            if (sequence.Select.OrderBy.Items.Count > 0)
            {
                if (sequence.Select.Select.TakeValue == null && sequence.Select.Select.SkipValue == null)
                    sequence.Select.OrderBy.Items.Clear();
                else
                    sequence = new SubQueryContext(sequence);
            }

            var context = new AggregationContext(buildInfo.Parent, sequence, methodCall);
            var sql = sequence.ConvertToSql(null, 0, ConvertFlags.Field).Select(_ => _.Sql).ToArray();

            if (sql.Length == 1 && sql[0] is ISelectQuery)
            {
                var query = (ISelectQuery) sql[0];

                if (query.Select.Columns.Count == 1)
                {
                    var join = SelectQuery.OuterApply(query);
                    context.Select.From.Tables.First.Value.Joins.AddLast(join.JoinedTable);
                    sql[0] = query.Select.Columns[0];
                }
            }

            context.Sql = context.Select;
            context.FieldIndex = context.Select.Select.Add(
                new SqlFunction(methodCall.Type, methodCall.Method.Name, sql));

            return context;
        }

        protected override SequenceConvertInfo Convert(
            ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo, ParameterExpression param)
        {
            return null;
        }

        private class AggregationContext : SequenceContextBase
        {
            private readonly string _methodName;
            private readonly Type _returnType;
            private SqlInfo[] _index;

            public int FieldIndex;
            public IQueryExpression Sql;

            public AggregationContext(IBuildContext parent, IBuildContext sequence, MethodCallExpression methodCall)
                : base(parent, sequence, null)
            {
                _returnType = methodCall.Method.ReturnType;
                _methodName = methodCall.Method.Name;
            }

            private static int CheckNullValue(IDataRecord reader, object context)
            {
                if (reader.IsDBNull(0))
                    throw new InvalidOperationException(
                        "Function {0} returns non-nullable value, but result is NULL. Use nullable version of the function instead."
                            .Args(context));
                return 0;
            }

            public override void BuildQuery<T>(Query<T> query, ParameterExpression queryParameter)
            {
                var expr = BuildExpression(FieldIndex);
                var mapper = Builder.BuildMapper<object>(expr);

                query.SetElementQuery(mapper.Compile());
            }

            public override Expression BuildExpression(Expression expression, int level)
            {
                return BuildExpression(ConvertToIndex(expression, level, ConvertFlags.Field)[0].Index);
            }

            private Expression BuildExpression(int fieldIndex)
            {
                Expression expr;

                if (_returnType.IsClassEx() || _methodName == "Sum" || _returnType.IsNullable())
                {
                    expr = Builder.BuildSql(_returnType, fieldIndex);
                }
                else
                {
                    expr = Expression.Block(
                        Expression.Call(null, MemberHelper.MethodOf(() => CheckNullValue(null, null)),
                            ExpressionBuilder.DataReaderParam, Expression.Constant(_methodName)),
                        Builder.BuildSql(_returnType, fieldIndex));
                }

                return expr;
            }

            public override SqlInfo[] ConvertToSql(Expression expression, int level, ConvertFlags flags)
            {
                switch (flags)
                {
                    case ConvertFlags.All:
                    case ConvertFlags.Key:
                    case ConvertFlags.Field:
                        return Sequence.ConvertToSql(expression, level + 1, flags);
                }

                throw new InvalidOperationException();
            }

            public override SqlInfo[] ConvertToIndex(Expression expression, int level, ConvertFlags flags)
            {
                switch (flags)
                {
                    case ConvertFlags.Field:
                        return _index ?? (_index = new[]
                        {
                            new SqlInfo {Query = Parent.Select, Index = Parent.Select.Select.Add(Sql), Sql = Sql}
                        });
                }

                throw new InvalidOperationException();
            }

            public override IsExpressionResult IsExpression(Expression expression, int level, RequestFor requestFlag)
            {
                switch (requestFlag)
                {
                    case RequestFor.Root:
                        return new IsExpressionResult(Lambda != null && expression == Lambda.Parameters[0]);
                    case RequestFor.Expression:
                        return IsExpressionResult.True;
                }

                return IsExpressionResult.False;
            }

            public override IBuildContext GetContext(Expression expression, int level, BuildInfo buildInfo)
            {
                throw new NotImplementedException();
            }
        }
    }
}