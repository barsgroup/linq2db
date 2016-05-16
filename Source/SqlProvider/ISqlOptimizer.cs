using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlProvider
{
    public interface ISqlOptimizer
    {
        ISelectQuery Finalize(ISelectQuery selectQuery);
        IQueryExpression ConvertExpression(IQueryExpression expression);
        ISqlPredicate ConvertPredicate(ISelectQuery selectQuery, ISqlPredicate predicate);
    }
}