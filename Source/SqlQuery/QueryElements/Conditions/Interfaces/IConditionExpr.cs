using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces
{
    public interface IConditionExpr<out T>
    {
        T Expr(IQueryExpression expr);
        T Field(ISqlField field);
    }
}