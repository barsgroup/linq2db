using System.Collections.Generic;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Visitors.Entities;

namespace Bars2Db.Linq.Joiner.Visitors.Interfaces
{
    /// <summary>Интерфейс обработчико узлов MethodCall в дереве выражений</summary>
    public interface IMethodCallHandler
    {
        /// <summary>Проверка может ли хендлер обработать узел</summary>
        /// <param name="node">Текущий methodCall</param>
        /// <returns>Результат проверк</returns>
        bool CanHandle(MethodCallExpression node);

        /// <summary>Возвращает выражения запросов, которые входят в текущий MethodCall</summary>
        /// <param name="methodCall">Текущий methodCall</param>
        /// <returns>Запросы которые вошли в текущий запрос</returns>
        IEnumerable<Expression> GetNextQueries(MethodCallExpression methodCall);

        /// <summary>Получить сотвествия между путями текущего запроса, и путями запросов пришедших в него</summary>
        /// <param name="methodCall">Текущий methodCall</param>
        /// <returns>Соответствия между путями текущего запроса и путями запросов пришедших в него</returns>
        IEnumerable<FullPathBinding> GetPathBindings(MethodCallExpression methodCall);

        /// <summary>Получить пути используемые в запросе</summary>
        /// <param name="queryExpression">Текущий узел</param>
        /// <returns>Все пути использумые в запросе</returns>
        ISet<FullPathInfo> GetPaths(MethodCallExpression queryExpression);
    }
}