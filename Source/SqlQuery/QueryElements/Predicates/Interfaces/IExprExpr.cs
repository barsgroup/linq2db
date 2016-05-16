using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces
{
    public interface IExprExpr : IExpr
    {
        EOperator EOperator { get; }

        [SearchContainer]
        IQueryExpression Expr2 { get; set; }
    }
}