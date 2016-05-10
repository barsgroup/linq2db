using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces
{
    public interface ILike : INotExpr
    {
        [SearchContainer]
        IQueryExpression Expr2 { get; set; }

        [SearchContainer]
        IQueryExpression Escape { get; set; }

        string GetOperator();
    }
}