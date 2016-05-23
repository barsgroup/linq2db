using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Visitors.Entities;
using Bars2Db.Linq.Joiner.Visitors.Interfaces;

namespace Bars2Db.Linq.Joiner.Visitors.Handlers
{
    public class GroupByWithSelectorMethodCallHandler : MethodCallHandler
    {
        public ILambdaExpressionHelper LambdaExpressionHelper { get; set; }

        /// <summary>Получить сотвествия между путями текущего запроса, и путями запросов пришедших в него</summary>
        /// <param name="methodCall">Текущий methodCall</param>
        /// <returns>Соответствия между путями текущего запроса и путями запросов пришедших в него</returns>
        public override IEnumerable<FullPathBinding> GetPathBindings(MethodCallExpression methodCall)
        {
            return new[] { LambdaExpressionHelper.CreateDefaultBinding(methodCall) };
        }

        protected override bool CanHandleMethod(MethodCallExpression method)
        {
            return method.Method.Name == "GroupBy" && method.GetLamdaArguments().Count() == 2;
        }

        protected override IEnumerable<FullPathInfo> GetAllMemberPaths(MethodCallExpression methodCall)
        {
            var lambdaExpressions = methodCall.GetLamdaArguments().ToArray();

            var groupBySelector = lambdaExpressions.First();

            var nextQuery = methodCall.Arguments[0];

            var groupBySelectorPaths = LambdaExpressionHelper.GetAllMemberAccessPaths(groupBySelector, nextQuery);

            var resultSelector = lambdaExpressions.Last();

            if (resultSelector.Parameters.Count == 1)
            {
                return groupBySelectorPaths.Union(LambdaExpressionHelper.GetAllMemberAccessPaths(resultSelector, nextQuery));
            }
            
            return groupBySelectorPaths.Union(LambdaExpressionHelper.GetAllMemberAccessPaths(resultSelector, nextQuery, null));
        }

        protected override IEnumerable<Expression> GetNextQueriesInternal(MethodCallExpression methodCall)
        {
            yield return methodCall.Arguments[0];
        }
    }
}