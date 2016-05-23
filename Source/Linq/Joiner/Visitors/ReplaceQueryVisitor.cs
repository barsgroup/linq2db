using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bars2Db.Linq.Joiner.Visitors
{
    /// <summary>Визитор для замены констант запросов в деревьях выражений</summary>
    public class ReplaceQueryVisitor : ExpressionVisitor
    {
        private readonly Expression _expresion;

        private readonly IReadOnlyDictionary<Expression, IQueryable> _replaceQueryDictionary;

        /// <summary>Initializes a new instance of <see cref="T:System.Linq.Expressions.ExpressionVisitor" />.</summary>
        public ReplaceQueryVisitor(IReadOnlyDictionary<Expression, IQueryable> replaceQueryDictionary, Expression expression)
        {
            _replaceQueryDictionary = replaceQueryDictionary;
            _expresion = expression;
        }

        public Expression Visit()
        {
            return Visit(_expresion);
        }

        /// <summary>Visits the <see cref="T:System.Linq.Expressions.ConstantExpression" />.</summary>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        /// <param name="node">The expression to visit.</param>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            IQueryable query;
            if (_replaceQueryDictionary.TryGetValue(node, out query))
            {
                return query.Expression;
            }

            return base.VisitConstant(node);
        }

        /// <summary>Visits the children of the <see cref="T:System.Linq.Expressions.MethodCallExpression" />.</summary>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        /// <param name="node">The expression to visit.</param>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (SmartJoinerExtensions.IsMapMethodCall(node.Method))
            {
                return Visit(node.Arguments[0]);
            }

            return base.VisitMethodCall(node);
        }
    }
}