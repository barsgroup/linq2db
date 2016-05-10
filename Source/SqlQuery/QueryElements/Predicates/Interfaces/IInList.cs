using System.Collections.Generic;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces
{
    public interface IInList : INotExpr
    {
        [SearchContainer]
        List<IQueryExpression> Values { get; }
    }
}