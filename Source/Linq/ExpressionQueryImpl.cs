using System.Linq.Expressions;
using Bars2Db.Linq.Interfaces;

namespace Bars2Db.Linq
{
    internal class ExpressionQueryImpl<T> : ExpressionQuery<T>, IExpressionQuery
    {
        public ExpressionQueryImpl(IDataContextInfo dataContext, Expression expression)
        {
            Init(dataContext, expression);
        }

        public override string ToString()
        {
            return SqlText;
        }
    }
}