using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Visitors.Entities;
using Bars2Db.Linq.Joiner.Visitors.Interfaces;

namespace Bars2Db.Linq.Joiner.Visitors.Handlers
{
    public abstract class MethodCallHandler : IMethodCallHandler
    {
        public bool CanHandle(MethodCallExpression node)
        {
            if (node.NodeType != ExpressionType.Call)
            {
                return false;
            }

            return CanHandleMethod(node);
        }

        public ISet<FullPathInfo> GetPaths(MethodCallExpression queryExpression)
        {
            var members = GetAllMemberPaths(queryExpression);

            return new HashSet<FullPathInfo>(members);
        }

        public virtual IEnumerable<Expression> GetNextQueries(MethodCallExpression methodCall)
        {
            return GetNextQueriesInternal(methodCall).Concat(methodCall.GetLamdaArguments().SelectMany(a => a.GetQueryableCallExpressions()));
        }

        protected virtual IEnumerable<Expression> GetNextQueriesInternal(MethodCallExpression methodCall)
        {
            yield return methodCall.Arguments[0];
        }

        public abstract IEnumerable<FullPathBinding> GetPathBindings(MethodCallExpression methodCall);

        protected abstract bool CanHandleMethod(MethodCallExpression method);

        protected abstract IEnumerable<FullPathInfo> GetAllMemberPaths(MethodCallExpression methodCall);
    }
}