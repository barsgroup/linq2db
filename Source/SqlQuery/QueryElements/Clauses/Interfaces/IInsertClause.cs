using System.Collections.Generic;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Clauses.Interfaces
{
    public interface IInsertClause : IQueryElement,
        ISqlExpressionWalkable,
        ICloneableElement
    {
        [SearchContainer]
        LinkedList<ISetExpression> Items { get; }

        [SearchContainer]
        ISqlTable Into { get; set; }

        bool WithIdentity { get; set; }
    }
}