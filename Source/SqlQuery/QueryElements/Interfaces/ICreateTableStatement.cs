using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Interfaces
{
    public interface ICreateTableStatement : IQueryElement,
        ISqlExpressionWalkable,
        ICloneableElement
    {
        [SearchContainer]
        ISqlTable Table { get; set; }

        bool IsDrop { get; set; }

        string StatementHeader { get; set; }

        string StatementFooter { get; set; }

        EDefaulNullable EDefaulNullable { get; set; }
    }
}