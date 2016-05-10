using Bars2Db.SqlQuery.QueryElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces
{
    public interface IConditionBase<T1, out T2> : IConditionExpr<IExpr<T1, T2>>,
        IQueryElement
        where T1 : IConditionBase<T1, T2>
    {
        INot<T1, T2> Not { get; }

        ISearchCondition Search { get; }

        bool IsEmpty { get; }
        T2 GetNext();

        T1 SetOr(bool value);

        T2 Exists(ISelectQuery subQuery);
    }
}