using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Seterlund.CodeGuard;
using Seterlund.CodeGuard.Validators;

namespace Bars2Db.Linq.Joiner
{
    public static class SmartJoinerExtensions
    {
        /// <summary>Кэширующее поле для метода Map</summary>
        private static MethodInfo mapMethodInfo;

        /// <summary>Проверяет, является ли переданный метод вызовом метода Map</summary>
        /// <param name="methodInfo">Информация о методе</param>
        public static bool IsMapMethodCall(MethodInfo methodInfo)
        {
            mapMethodInfo = mapMethodInfo ?? typeof(SmartJoinerExtensions).GetMethod("Map", BindingFlags.Static | BindingFlags.Public);
            return methodInfo.IsGenericMethod && methodInfo.GetGenericArguments().Length == 2 &&
                   mapMethodInfo.GetGenericMethodDefinition() == methodInfo.GetGenericMethodDefinition();
        }

        public static IQueryable<TEntity> Map<TEntity, TPropertyEntity>(this IQueryable<TEntity> source,
                                                                        Expression<Func<TEntity, TPropertyEntity>> propertyExpressionFunc,
                                                                        IQueryable<TPropertyEntity> propertyQuery)
        {
            Guard.That(source).IsNotNull();
            Guard.That(propertyExpressionFunc).IsNotNull();
            Guard.That(propertyQuery).IsNotNull();

            var methodInfo = GetMethodInfo<TEntity, TPropertyEntity>(Map);

            return source.Provider.CreateQuery<TEntity>(Expression.Call(null, methodInfo, new[] { source.Expression, propertyExpressionFunc, Expression.Constant(propertyQuery) }));
        }

        private static MethodInfo GetMethodInfo<T1, T2>(Func<IQueryable<T1>, Expression<Func<T1, T2>>, IQueryable<T2>, IQueryable<T1>> func)
        {
            return func.Method;
        }

        public static IEnumerable<Expression> GetQueryableCallExpressions(this Expression expression)
        {
            var queryCallExpressionVisitor = new QueryCallExpressionVisitor();

            return queryCallExpressionVisitor.GetExpressions(expression);
        }

        public static IEnumerable<LambdaExpression> GetLamdaArguments(this MethodCallExpression methodCall)
        {
            return
                methodCall.Arguments.Where(x => x.NodeType == ExpressionType.Quote)
                          .Select(x => ((UnaryExpression)x).Operand)
                          .Where(x => x.NodeType == ExpressionType.Lambda)
                          .Select(lambdaExpression => (LambdaExpression)lambdaExpression);
        }


        private class QueryCallExpressionVisitor : ExpressionVisitor
        {
            private readonly Collection<Expression> _queryableCallExpressions = new Collection<Expression>();

            public IReadOnlyCollection<Expression> GetExpressions(Expression expression)
            {
                Visit(expression);

                return _queryableCallExpressions;
            }

            /// <summary>Dispatches the expression to one of the more specialized visit methods in this class.</summary>
            /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
            /// <param name="node">The expression to visit.</param>
            public override Expression Visit(Expression node)
            {
                if (node == null)
                {
                    return null;
                }

                var methodCall = node as MethodCallExpression;

                // если это вызов метода перед которым идет подзав
                if (methodCall != null)
                {
                    var nextRoot = methodCall.Arguments.FirstOrDefault();

                    var isScalarMethodCall = nextRoot != null && typeof(IQueryable).IsAssignableFrom(nextRoot.Type);

                    if (isScalarMethodCall)
                    {
                        _queryableCallExpressions.Add(node);

                        return node;
                    }
                }
                else if (typeof(IQueryable).IsAssignableFrom(node.Type))
                {
                    _queryableCallExpressions.Add(node);

                    return node;
                }

                return base.Visit(node);
            }
        }
    }
}