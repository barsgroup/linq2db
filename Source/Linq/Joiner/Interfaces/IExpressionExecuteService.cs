using System.Collections.Generic;
using System.Linq.Expressions;

namespace Bars2Db.Linq.Joiner.Interfaces
{
    /// <summary>Сервис для выполнения деревьев выражений</summary>
    public interface IExpressionExecuteService
    {
        /// <summary>Выполнить expression</summary>
        IEnumerable<TEntity> Execute<TEntity>(Expression expressionWithJoines);

        /// <summary>Получить элемент</summary>
        /// <param name="expression">Дерево выражений</param>
        object Execute(Expression expression);
    }
}