using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces
{
    public interface IHierarhicalPredicate : IExpr
    {
        [SearchContainer]
        IQueryExpression Expr2 { get; set; }

        HierarhicalFlow Flow { get; }

        string GetOperator();
    }
}