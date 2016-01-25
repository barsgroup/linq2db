namespace LinqToDB.SqlQuery.QueryElements.Conditions
{
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.SqlElements;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public abstract class ConditionBase<T1,T2> : BaseQueryElement, IConditionExpr<ConditionBase<T1,T2>.Expr_>
        where T1 : ConditionBase<T1,T2>
    {
        public class Expr_
        {
            internal Expr_(ConditionBase<T1,T2> condition, bool isNot, ISqlExpression expr)
            {
                _condition = condition;
                _isNot     = isNot;
                _expr      = expr;
            }

            readonly ConditionBase<T1,T2> _condition;
            readonly bool                 _isNot;
            readonly ISqlExpression       _expr;

            T2 Add(ISqlPredicate predicate)
            {
                _condition.Search.Conditions.Add(new Condition(_isNot, predicate));
                return _condition.GetNext();
            }

            #region Predicate.ExprExpr

            public class Op_ : IConditionExpr<T2>
            {
                internal Op_(Expr_ expr, Operator op) 
                {
                    _expr = expr;
                    _op   = op;
                }

                readonly Expr_              _expr;
                readonly Operator _op;

                public T2 Expr    (ISqlExpression expr)       { return _expr.Add(new ExprExpr(_expr._expr, _op, expr)); }
                public T2 Field   (SqlField      field)       { return Expr(field);               }
                public T2 SubQuery(SelectQuery   selectQuery) { return Expr(selectQuery);         }
                public T2 Value   (object        value)       { return Expr(new SqlValue(value)); }

                public T2 All     (SelectQuery   subQuery)    { return Expr(SqlFunction.CreateAll (subQuery)); }
                public T2 Some    (SelectQuery   subQuery)    { return Expr(SqlFunction.CreateSome(subQuery)); }
                public T2 Any     (SelectQuery   subQuery)    { return Expr(SqlFunction.CreateAny (subQuery)); }
            }

            public Op_ Equal => new Op_(this, Operator.Equal);

            public Op_ NotEqual => new Op_(this, Operator.NotEqual);

            public Op_ Greater => new Op_(this, Operator.Greater);

            public Op_ GreaterOrEqual => new Op_(this, Operator.GreaterOrEqual);

            public Op_ NotGreater => new Op_(this, Operator.NotGreater);

            public Op_ Less => new Op_(this, Operator.Less);

            public Op_ LessOrEqual => new Op_(this, Operator.LessOrEqual);

            public Op_ NotLess => new Op_(this, Operator.NotLess);

            #endregion

            #region Predicate.Like

            public T2 Like(ISqlExpression expression, SqlValue escape) { return Add(new Like(_expr, false, expression, escape)); }
            public T2 Like(ISqlExpression expression)                  { return Like(expression, null); }
            public T2 Like(string expression,         SqlValue escape) { return Like(new SqlValue(expression), escape); }
            public T2 Like(string expression)                          { return Like(new SqlValue(expression), null);   }

            #endregion

            #region Predicate.Between

            public T2 Between   (ISqlExpression expr1, ISqlExpression expr2) { return Add(new Between(_expr, false, expr1, expr2)); }
            public T2 NotBetween(ISqlExpression expr1, ISqlExpression expr2) { return Add(new Between(_expr, true,  expr1, expr2)); }

            #endregion

            #region Predicate.IsNull

            public T2 IsNull => Add(new IsNull(_expr, false));

            public T2 IsNotNull => Add(new IsNull(_expr, true));

            #endregion

            #region Predicate.In

            public T2 In   (SelectQuery subQuery) { return Add(new InSubQuery(_expr, false, subQuery)); }
            public T2 NotIn(SelectQuery subQuery) { return Add(new InSubQuery(_expr, true,  subQuery)); }

            InList CreateInList(bool isNot, object[] exprs)
            {
                var list = new InList(_expr, isNot, null);

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

        public class Not_ : IConditionExpr<Expr_>
        {
            internal Not_(ConditionBase<T1,T2> condition)
            {
                _condition = condition;
            }

            readonly ConditionBase<T1,T2> _condition;

            public Expr_ Expr    (ISqlExpression expr)        { return new Expr_(_condition, true, expr); }
            public Expr_ Field   (SqlField       field)       { return Expr(field);               }
            public Expr_ SubQuery(SelectQuery    selectQuery) { return Expr(selectQuery);            }
            public Expr_ Value   (object         value)       { return Expr(new SqlValue(value)); }

            public T2 Exists(SelectQuery subQuery)
            {
                _condition.Search.Conditions.Add(new Condition(true, new FuncLike(SqlFunction.CreateExists(subQuery))));
                return _condition.GetNext();
            }
        }

        protected abstract SearchCondition Search { get; }
        protected abstract T2              GetNext();

        public T1 SetOr(bool value)
        {
            Search.Conditions[Search.Conditions.Count - 1].IsOr = value;
            return (T1)this;
        }

        public Not_  Not => new Not_(this);

        public Expr_ Expr    (ISqlExpression expr)        { return new Expr_(this, false, expr); }
        public Expr_ Field   (SqlField       field)       { return Expr(field);                  }
        public Expr_ SubQuery(SelectQuery    selectQuery) { return Expr(selectQuery);            }
        public Expr_ Value   (object         value)       { return Expr(new SqlValue(value));    }

        public T2 Exists(SelectQuery subQuery)
        {
            Search.Conditions.Add(new Condition(false, new FuncLike(SqlFunction.CreateExists(subQuery))));
            return GetNext();
        }
    }
}