using Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Predicates;
using Bars2Db.SqlQuery.QueryElements.SqlElements;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Conditions
{
    internal class Not<T1, T2> : INot<T1, T2>
        where T1 : IConditionBase<T1, T2>
    {
        private readonly IConditionBase<T1, T2> _condition;

        internal Not(IConditionBase<T1, T2> condition)
        {
            _condition = condition;
        }

        public IExpr<T1, T2> Expr(IQueryExpression expr)
        {
            return new Expr<T1, T2>(_condition, true, expr);
        }

        public IExpr<T1, T2> Field(ISqlField field)
        {
            return Expr(field);
        }

        public T2 Exists(ISelectQuery subQuery)
        {
            _condition.Search.Conditions.AddLast(new Condition(true, new FuncLike(SqlFunction.CreateExists(subQuery))));
            return _condition.GetNext();
        }
    }
}