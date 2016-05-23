using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Visitors.Entities;

namespace Bars2Db.Linq.Joiner.Visitors.Handlers
{
    /// <summary>Обработчик для узлов результатом которых не является запрос</summary>
    public class ScalarMethodHandler : BaseFilterMethodCallHandler
    {
        public override IEnumerable<FullPathBinding> GetPathBindings(MethodCallExpression methodCall)
        {
            return Enumerable.Empty<FullPathBinding>();
        }

        protected override bool CanHandleMethod(MethodCallExpression method)
        {
            return !typeof(IQueryable).IsAssignableFrom(method.Type);
        }
    }
}