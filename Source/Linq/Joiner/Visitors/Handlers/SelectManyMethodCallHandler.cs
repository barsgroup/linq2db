using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Visitors.Entities;
using Bars2Db.Linq.Joiner.Visitors.Interfaces;

namespace Bars2Db.Linq.Joiner.Visitors.Handlers
{
    public class SelectManyMethodCallHandler : MethodCallHandler
    {
        public ILambdaExpressionHelper LambdaExpressionHelper { get; set; }

        /// <summary>Получить сотвествия между путями текущего запроса, и путями запросов пришедших в него</summary>
        /// <param name="methodCall">Текущий methodCall</param>
        /// <returns>Соответствия между путями текущего запроса и путями запросов пришедших в него</returns>
        public override IEnumerable<FullPathBinding> GetPathBindings(MethodCallExpression methodCall)
        {
            var baseResult = LambdaExpressionHelper.GetBindingsFromResultSelector(methodCall, GetResultSelector(methodCall), GetSelectorLeftRoot(methodCall), GetRightSelectorRoot(methodCall));

            var nextQueryGroupJoin = GetNextRoot(methodCall) as MethodCallExpression;

            //АДИЩЕ если был GroupJoin до этого
            if (nextQueryGroupJoin != null && nextQueryGroupJoin.Method.Name == "GroupJoin")
            {
                var fromRightGroup = baseResult.Where(x => x.NewQueryPath.Root == GetRightSelectorRoot(methodCall));

                var fromLeftGroup = baseResult.Where(x => x.NewQueryPath.Root == GetSelectorLeftRoot(methodCall));

                var rightAfterRebuild = RebuildRightPaths(methodCall, fromRightGroup);

                return fromLeftGroup.Union(rightAfterRebuild);
            }

            return baseResult;
        }

        protected override bool CanHandleMethod(MethodCallExpression method)
        {
            return method.Method.Name == "SelectMany";
        }

        protected override IEnumerable<FullPathInfo> GetAllMemberPaths(MethodCallExpression methodCall)
        {
            var resultSelector = LambdaExpressionHelper.GetAllMemberAccessPaths(GetResultSelector(methodCall), GetSelectorLeftRoot(methodCall), GetRightSelectorRoot(methodCall));

            return resultSelector;
        }

        private static Expression GetSelectorLeftRoot(MethodCallExpression currentNode)
        {
            return currentNode.Arguments[0];
        }

        private Expression GetNextRoot(MethodCallExpression node)
        {
            return node.Arguments[0];
        }

        private static LambdaExpression GetResultSelector(MethodCallExpression methodCall)
        {
            return methodCall.GetLamdaArguments().Last();
        }

        private static Expression GetRightSelectorRoot(MethodCallExpression currentNode)
        {
            var groupSelector = currentNode.GetLamdaArguments().First();

            return groupSelector.Body;
        }

        private IEnumerable<FullPathBinding> RebuildRightPaths(MethodCallExpression currentNode, IEnumerable<FullPathBinding> bindingsFromRight)
        {
            var nextQuery = GetNextRoot(currentNode);

            var rightGroupRoot = GetRightSelectorRoot(currentNode);

            var rightGroupStartPath = FullPathInfo.CreatePath(rightGroupRoot);

            var memberPathWithounDefaultIfEmpty = ((MethodCallExpression)rightGroupRoot).Arguments[0];

            var rightGroupStartPathFromNextQuery = FullPathInfo.CreatePath(nextQuery, memberPathWithounDefaultIfEmpty);

            foreach (var pathBinding in bindingsFromRight)
            {
                //полный путь после разворачивания по селектору правой стороны
                var path = FullPathInfo.ReplaceStartPart(pathBinding.NewQueryPath, rightGroupStartPath, rightGroupStartPathFromNextQuery);

                yield return FullPathBinding.Create(pathBinding.CurrentQueryPath, path);
            }
        }
    }
}