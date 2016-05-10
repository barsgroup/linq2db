using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Interfaces
{
    public interface ISetExpression : ISqlExpressionWalkable, ICloneableElement, IQueryElement
    {
        [SearchContainer]
        IQueryExpression Column { get; set; }

        [SearchContainer]
        IQueryExpression Expression { get; set; }
    }
}