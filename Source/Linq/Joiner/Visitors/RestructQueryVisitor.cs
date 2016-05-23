using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Bars2Db.Linq.Joiner.Visitors
{
    /// <summary>Визитор для замены констант запросов в деревьях выражений</summary>
    public class RestructQueryVisitor : ExpressionVisitor
    {
        private readonly Expression _expression;

        private readonly Func<Expression, Expression> _subQueryAction;

        private IReadOnlyCollection<Expression> _subQueryExpressions;

        public RestructQueryVisitor(Func<Expression, Expression> subQueryAction, Expression expression)
        {
            _subQueryAction = subQueryAction;
            _expression = expression;
        }

        public Expression Visit()
        {
            var subQueryVisitor = new ExecuteSubQueryVisitor();
            subQueryVisitor.VisitMethodCalls(_expression, out _subQueryExpressions);

            return Visit(_expression);
        }

        /// <summary>
        /// Visits the <see cref="T:System.Linq.Expressions.ConstantExpression"/>.
        /// </summary>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        /// <param name="node">The expression to visit.</param>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type.IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(SmartQuery<>))
            {
                return Expression.Constant(node.Value);
            }

            return base.VisitConstant(node);
        }

        public override Expression Visit(Expression node)
        {
            return _subQueryExpressions.Contains(node) ? _subQueryAction(node) : base.Visit(node);
        }

        private class ExecuteSubQueryVisitor : DirectMethodCallVisitor
        {
            private readonly Collection<Expression> _subQueryExpressions = new Collection<Expression>();

            public override Expression Visit(Expression node)
            {
                if (node == null || !typeof(IQueryable).IsAssignableFrom(node.Type) || node.NodeType == ExpressionType.Call && ((MethodCallExpression)node).Method.DeclaringType == typeof(Queryable))
                {
                    return base.Visit(node);
                }

                _subQueryExpressions.Add(node);

                return node;
            }

            public void VisitMethodCalls(Expression expression, out IReadOnlyCollection<Expression> subQueryExpressions)
            {
                VisitDirectMethodCalls(expression);
                subQueryExpressions = _subQueryExpressions;
            }

            protected override void HandleMethodCall(MethodCallExpression currentMethodCall, Expression nextQuery)
            {
                var lambdaArguments = currentMethodCall.GetLamdaArguments().Concat(FindQueryableArgument(currentMethodCall));

                foreach (var lambdaArgument in lambdaArguments)
                {
                    Visit(lambdaArgument);
                }
            }

            private IEnumerable<Expression> FindQueryableArgument(MethodCallExpression methodCall)
            {
                if (SmartJoinerExtensions.IsMapMethodCall(methodCall.Method))
                {
                    return Enumerable.Empty<Expression>();
                }

                return methodCall.Arguments.Skip(1).Where(x => typeof(IQueryable).IsAssignableFrom(x.Type));
            }
        }
    }
}