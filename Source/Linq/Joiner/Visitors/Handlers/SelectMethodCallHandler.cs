using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Visitors.Entities;
using Bars2Db.Linq.Joiner.Visitors.Interfaces;

namespace Bars2Db.Linq.Joiner.Visitors.Handlers
{
    public class SelectMethodCallHandler : MethodCallHandler
    {
        public ILambdaExpressionHelper LambdaExpressionHelper { get; set; }

        /// <summary>Получить сотвествия между путями текущего запроса, и путями запросов пришедших в него</summary>
        /// <param name="methodCall">Текущий methodCall</param>
        /// <returns>Соответствия между путями текущего запроса и путями запросов пришедших в него</returns>
        public override IEnumerable<FullPathBinding> GetPathBindings(MethodCallExpression methodCall)
        {
            return LambdaExpressionHelper.GetBindingsFromResultSelector(methodCall, methodCall.GetLamdaArguments().First(), methodCall.Arguments[0]);
        }

        protected override bool CanHandleMethod(MethodCallExpression method)
        {
            return method.Method.Name == "Select";
        }

        protected override IEnumerable<FullPathInfo> GetAllMemberPaths(MethodCallExpression methodCall)
        {
            return LambdaExpressionHelper.GetAllMemberAccessPaths(methodCall.GetLamdaArguments().First(), methodCall.Arguments[0]);
        }
    }
}