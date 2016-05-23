using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Visitors.Entities;

namespace Bars2Db.Linq.Joiner.Visitors.Handlers
{
    public class UnionOrConcatMethodCallHandler : MethodCallHandler
    {
        protected override IEnumerable<Expression> GetNextQueriesInternal(MethodCallExpression methodCall)
        {
            yield return methodCall.Arguments[0];
            yield return methodCall.Arguments[1];
        }

        public override IEnumerable<FullPathBinding> GetPathBindings(MethodCallExpression methodCall)
        {
            //после Union в запросе не может быть сущностей
            return Enumerable.Empty<FullPathBinding>();
        }

        protected override bool CanHandleMethod(MethodCallExpression method)
        {
            return method.Method.Name == "Union" || method.Method.Name == "Concat";
        }

        protected override IEnumerable<FullPathInfo> GetAllMemberPaths(MethodCallExpression methodCall)
        {
            //после Union и Concat не используются обращения к полям. 
            return Enumerable.Empty<FullPathInfo>();
        }
    }
}