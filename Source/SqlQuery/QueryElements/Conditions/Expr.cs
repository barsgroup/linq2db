namespace LinqToDB.SqlQuery.QueryElements.Conditions
{
    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class Expr<T1, T2> : IExpr<T1, T2>
        where T1 : IConditionBase<T1,T2>
    {
        internal Expr(IConditionBase<T1,T2> condition, bool isNot, IQueryExpression sqlExpression)
        {
            _condition = condition;
            _isNot     = isNot;
            SqlExpression      = sqlExpression;
        }

        readonly IConditionBase<T1,T2> _condition;
        readonly bool                 _isNot;

        public IQueryExpression SqlExpression
        {
            get;
        }

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