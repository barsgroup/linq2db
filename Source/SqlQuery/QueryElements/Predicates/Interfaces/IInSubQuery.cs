using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces
{
    public interface IInSubQuery : INotExpr
    {
        [SearchContainer]
        ISelectQuery SubQuery { get; }
    }
}