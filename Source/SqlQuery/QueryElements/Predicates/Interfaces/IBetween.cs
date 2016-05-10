using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces
{
    public interface IBetween : INotExpr
    {
        [SearchContainer]
        IQueryExpression Expr2 { get; set; }

        [SearchContainer]
        IQueryExpression Expr3 { get; set; }
    }
}