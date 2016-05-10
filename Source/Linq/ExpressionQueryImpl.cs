using System.Linq.Expressions;

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