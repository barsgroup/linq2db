using Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Predicates;
using Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Conditions
{
    public class Expr<T1, T2> : IExpr<T1, T2>
        where T1 : IConditionBase<T1, T2>
    {
        private readonly IConditionBase<T1, T2> _condition;
        private readonly bool _isNot;

        internal Expr(IConditionBase<T1, T2> condition, bool isNot, IQueryExpression sqlExpression)
        {
            _condition = condition;
            _isNot = isNot;
            SqlExpression = sqlExpression;
        }

        public IQueryExpression SqlExpression { get; }

        public T2 Add(ISqlPredicate predicate)
        {
            _condition.Search.Conditions.AddLast(new Condition(_isNot, predicate));
            return _condition.GetNext();
        }

        #region Predicate.ExprExpr

        public IOperator<T2> Equal => new Operator<T1, T2>(this, EOperator.Equal);

        public IOperator<T2> NotEqual => new Operator<T1, T2>(this, EOperator.NotEqual);

        public IOperator<T2> Greater => new Operator<T1, T2>(this, EOperator.Greater);

        #endregion

        #region Predicate.IsNull

        public T2 IsNull => Add(new IsNull(SqlExpression, false));

        public T2 IsNotNull => Add(new IsNull(SqlExpression, true));

        #endregion
    }
}