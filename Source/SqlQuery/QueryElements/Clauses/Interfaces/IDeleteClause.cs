using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Clauses.Interfaces
{
    public interface IDeleteClause : ISqlExpressionWalkable,
        ICloneableElement, IQueryElement
    {
        [SearchContainer]
        ISqlTable Table { get; set; }
    }
}