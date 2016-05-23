using System.Collections.Generic;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Visitors.Entities;

namespace Bars2Db.Linq.Joiner.Visitors.Interfaces
{
    /// <summary>Вспопогательный сервис для обработки LambdaExpression</summary>
    public interface ILambdaExpressionHelper
    {
        /// <summary>Возвращает всу пути обращений к полям из lambda</summary>
        IEnumerable<FullPathInfo> GetAllMemberAccessPaths(LambdaExpression selector, params Expression[] parametersToRoots);

        IEnumerable<FullPathBinding> GetBindingsFromResultSelector(MethodCallExpression currentRoot, LambdaExpression selector, params Expression[] parametersToRoots);

        FullPathBinding CreateDefaultBinding(MethodCallExpression currentRoot);
    }
}