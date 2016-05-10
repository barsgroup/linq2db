using System.Collections.Generic;
using Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Clauses.Interfaces
{
    public interface IFromClause : IClauseWithConditionBase, ISqlExpressionWalkable
    {
        ITableSource this[ISqlTableSource table] { get; }

        ITableSource this[ISqlTableSource table, string alias] { get; }

        [SearchContainer]
        LinkedList<ITableSource> Tables { get; }

        IFromClause Table(ISqlTableSource table, params IJoin[] joins);

        bool IsChild(ISqlTableSource table);

        ISqlTableSource FindTableSource(ISqlTable table);
    }
}