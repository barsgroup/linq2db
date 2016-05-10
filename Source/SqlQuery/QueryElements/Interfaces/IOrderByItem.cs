using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Interfaces
{
    public interface IOrderByItem : IQueryElement, ISqlExpressionWalkable, ICloneableElement
    {
        [SearchContainer]
        IQueryExpression Expression { get; set; }

        bool IsDescending { get; }
    }
}