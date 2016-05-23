using System.Linq.Expressions;

namespace Bars2Db.Linq.Joiner.Visitors
{
    /// <summary>Базовый класс для обхода выражения по прямой цепочке MethodCall(не переключаясь на MethodCall которые могут быть в параметрах или где-то еще)</summary>
    public abstract class DirectMethodCallVisitor : ExpressionVisitor
    {
        public void VisitDirectMethodCalls(Expression expressionNode)
        {
            if (expressionNode.NodeType != ExpressionType.Call)
            {
                return;
            }

            var currentQueryable = expressionNode;

            while (currentQueryable.NodeType == ExpressionType.Call)
            {
                var currentMethodCall = (MethodCallExpression)currentQueryable;

                var nextQuery = currentMethodCall.Arguments[0];

                HandleMethodCall(currentMethodCall, nextQuery);

                currentQueryable = nextQuery;
            }
        }

        protected abstract void HandleMethodCall(MethodCallExpression currentMethodCall, Expression nextQuery);
    }
}