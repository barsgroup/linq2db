using System.Collections;
using System.Linq.Expressions;

namespace Bars2Db.Linq
{
    public interface IExpressionQuery
    {
        string SqlText { get; }

        Query GetQuery();

        IEnumerable Execute(Expression expression);
    }
}