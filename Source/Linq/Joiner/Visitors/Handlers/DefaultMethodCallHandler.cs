using System.Collections.Generic;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Visitors.Entities;
using Bars2Db.Linq.Joiner.Visitors.Interfaces;

namespace Bars2Db.Linq.Joiner.Visitors.Handlers
{
    public class DefaultMethodCallHandler : BaseFilterMethodCallHandler, IDefaultMethodHandler
    {
        public override IEnumerable<FullPathBinding> GetPathBindings(MethodCallExpression methodCall)
        {
            return new[] { LambdaExpressionHelper.CreateDefaultBinding(methodCall)};
        }

        protected override bool CanHandleMethod(MethodCallExpression method)
        {
            return true;
        }
    }
}