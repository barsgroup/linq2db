using Bars2Db.SqlQuery.QueryElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces
{
    public interface INot<T1, out T2> : IConditionExpr<IExpr<T1, T2>>
        where T1 : IConditionBase<T1, T2>
    {
        T2 Exists(ISelectQuery subQuery);
    }
}