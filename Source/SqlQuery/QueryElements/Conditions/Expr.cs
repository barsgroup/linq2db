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
        internal Expr(IConditionBase<T1,T2> condition, bool isNot, ISqlExpression sqlExpression)
        {
            _condition = condition;
            _isNot     = isNot;
            SqlExpression      = sqlExpression;
        }

        readonly IConditionBase<T1,T2> _condition;
        readonly bool                 _isNot;

        public ISqlExpression SqlExpression
        {
            get;
        }

        public T2 Add(ISqlPredicate predicate)
        {
            _condition.Search.Conditions.Add(new Condition(_isNot, predicate));
            return _condition.GetNext();
        }

        #region Predicate.ExprExpr

        public IOperator<T2> Equal => new Operator<T1, T2>(this, EOperator.Equal);

        public IOperator<T2> NotEqual => new Operator<T1, T2>(this, EOperator.NotEqual);

        public IOperator<T2> Greater => new Operator<T1, T2>(this, EOperator.Greater);

        public IOperator<T2> GreaterOrEqual => new Operator<T1, T2>(this, EOperator.GreaterOrEqual);

        public IOperator<T2> NotGreater => new Operator<T1, T2>(this, EOperator.NotGreater);

        public IOperator<T2> Less => new Operator<T1, T2>(this, EOperator.Less);

        public IOperator<T2> LessOrEqual => new Operator<T1, T2>(this, EOperator.LessOrEqual);

        public IOperator<T2> NotLess => new Operator<T1, T2>(this, EOperator.NotLess);
                             
        #endregion

        #region Predicate.Like

        public T2 Like(ISqlExpression expression, SqlValue escape) { return Add(new Like(SqlExpression, false, expression, escape)); }
        public T2 Like(ISqlExpression expression)                  { return Like(expression, null); }
        public T2 Like(string expression,         SqlValue escape) { return Like(new SqlValue(expression), escape); }
        public T2 Like(string expression)                          { return Like(new SqlValue(expression), null);   }

        #endregion

        #region Predicate.Between

        public T2 Between   (ISqlExpression expr1, ISqlExpression expr2) { return Add(new Between(SqlExpression, false, expr1, expr2)); }
        public T2 NotBetween(ISqlExpression expr1, ISqlExpression expr2) { return Add(new Between(SqlExpression, true,  expr1, expr2)); }

        #endregion

        #region Predicate.IsNull

        public T2 IsNull => Add(new IsNull(SqlExpression, false));

        public T2 IsNotNull => Add(new IsNull(SqlExpression, true));

        #endregion

        #region Predicate.In

        public T2 In   (ISelectQuery subQuery) { return Add(new InSubQuery(SqlExpression, false, subQuery)); }
        public T2 NotIn(ISelectQuery subQuery) { return Add(new InSubQuery(SqlExpression, true,  subQuery)); }

        IInList CreateInList(bool isNot, object[] exprs)
        {
            var list = new InList(SqlExpression, isNot, null);

            if (exprs != null && exprs.Length > 0)
            {
                foreach (var item in exprs)
                {
                    if (item == null || item is SqlValue && ((SqlValue)item).Value == null)
                        continue;

                    if (item is ISqlExpression)
                        list.Values.Add((ISqlExpression)item);
                    else
                        list.Values.Add(new SqlValue(item));
                }
            }

            return list;
        }

        public T2 In   (params object[] exprs) { return Add(CreateInList(false, exprs)); }
        public T2 NotIn(params object[] exprs) { return Add(CreateInList(true,  exprs)); }

        #endregion
    }
}