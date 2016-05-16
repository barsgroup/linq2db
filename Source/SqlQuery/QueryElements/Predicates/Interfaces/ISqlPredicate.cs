using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces
{
    public interface ISqlPredicate : IQueryElement, ISqlExpressionWalkable, ICloneableElement, IOperation
    {
    }
}