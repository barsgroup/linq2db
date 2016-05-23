using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Visitors.Entities;
using Bars2Db.Linq.Joiner.Visitors.Interfaces;

namespace Bars2Db.Linq.Joiner.Visitors.Handlers
{
    public class GroupByWithoutSelectorMethodCallHandler : MethodCallHandler
    {
        public ILambdaExpressionHelper LambdaExpressionHelper { get; set; }

        /// <summary>Получить сотвествия между путями текущего запроса, и путями запросов пришедших в него</summary>
        /// <param name="methodCall">Текущий methodCall</param>
        /// <returns>Соответствия между путями текущего запроса и путями запросов пришедших в него</returns>
        public override IEnumerable<FullPathBinding> GetPathBindings(MethodCallExpression methodCall)
        {
            //не может быть сущностей после GroupBy
            return Enumerable.Empty<FullPathBinding>();
        }

        protected override bool CanHandleMethod(MethodCallExpression method)
        {
            return method.Method.Name == "GroupBy" && method.GetLamdaArguments().Count() == 1;
        }

        protected override IEnumerable<FullPathInfo> GetAllMemberPaths(MethodCallExpression methodCall)
        {
            return LambdaExpressionHelper.GetAllMemberAccessPaths(methodCall.GetLamdaArguments().First(), methodCall.Arguments[0]);
        }

        protected override IEnumerable<Expression> GetNextQueriesInternal(MethodCallExpression methodCall)
        {
            yield return methodCall.Arguments[0];
        }
    }
}