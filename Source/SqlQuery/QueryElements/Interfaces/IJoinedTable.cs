using Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Interfaces
{
    public interface IJoinedTable : ISqlExpressionWalkable, ICloneableElement, IQueryElement
    {
        EJoinType JoinType { get; set; }

        [SearchContainer]
        ITableSource Table { get; set; }

        [SearchContainer]
        ISearchCondition Condition { get; }

        bool IsWeak { get; set; }

        bool CanConvertApply { get; set; }
    }
}