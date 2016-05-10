using System.Collections.Generic;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Clauses.Interfaces
{
    public interface IUpdateClause : IQueryElement,
        ISqlExpressionWalkable,
        ICloneableElement
    {
        [SearchContainer]
        LinkedList<ISetExpression> Items { get; }

        [SearchContainer]
        LinkedList<ISetExpression> Keys { get; }

        [SearchContainer]
        ISqlTable Table { get; set; }
    }
}