using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Visitors.Entities;
using Bars2Db.Linq.Joiner.Visitors.Interfaces;

namespace Bars2Db.Linq.Joiner.Visitors.Handlers
{
    /// <summary>Базовыф обработчик для метод фильтрующих запрос</summary>
    public abstract class BaseFilterMethodCallHandler : MethodCallHandler
    {
        public ILambdaExpressionHelper LambdaExpressionHelper { get; set; }

        protected override IEnumerable<FullPathInfo> GetAllMemberPaths(MethodCallExpression methodCall)
        {
            var filterLambda = methodCall.GetLamdaArguments().SingleOrDefault();

            if (filterLambda != null)
            {
                var paths = LambdaExpressionHelper.GetAllMemberAccessPaths(filterLambda, methodCall.Arguments[0]);

                return paths;
            }

            return Enumerable.Empty<FullPathInfo>();
        }
    }
}