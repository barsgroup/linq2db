using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces
{
    public interface IFuncLike : ISqlPredicate
    {
        [SearchContainer]
        ISqlFunction Function { get; }
    }
}