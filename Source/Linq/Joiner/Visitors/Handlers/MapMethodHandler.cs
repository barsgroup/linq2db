using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Visitors.Entities;

namespace Bars2Db.Linq.Joiner.Visitors.Handlers
{
    public class MapMethodHandler : DefaultMethodCallHandler
    {
        public override IEnumerable<FullPathBinding> GetPathBindings(MethodCallExpression methodCall)
        {
            var nextRoot = methodCall.Arguments[0];

            var currentPath = GetMapMemberPath(methodCall);

            var newPath = currentPath.Copy(nextRoot);

            var pathBinding = FullPathBinding.Create(currentPath, newPath);

            return new[] { pathBinding, LambdaExpressionHelper.CreateDefaultBinding(methodCall) };
        }

        protected override bool CanHandleMethod(MethodCallExpression method)
        {
            return SmartJoinerExtensions.IsMapMethodCall(method.Method);
        }

        protected override IEnumerable<FullPathInfo> GetAllMemberPaths(MethodCallExpression methodCall)
        {
            return new[] { GetMapMemberPath(methodCall) };
        }

        private FullPathInfo GetMapMemberPath(MethodCallExpression methodCall)
        {
            var queryableConstant = (ConstantExpression)methodCall.Arguments[2];

            var mapMemberPath = FullPathInfo.CreatePath(methodCall.Arguments[0], methodCall.GetLamdaArguments().First().Body);

            mapMemberPath.MapQueryable = (IQueryable)queryableConstant.Value;

            return mapMemberPath;
        }
    }
}