namespace LinqToDB.SqlQuery.QueryElements.Conditions
{
    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    class Not<T1, T2> : INot<T1, T2>
        where T1 : IConditionBase<T1,T2>
    {
        internal Not(IConditionBase<T1,T2> condition)
        {
            _condition = condition;
        }

        readonly IConditionBase<T1,T2> _condition;

        public IExpr<T1, T2> Expr    (ISqlExpression expr)        { return new Expr<T1, T2>(_condition, true, expr); }
        public IExpr<T1, T2> Field   (SqlField       field)       { return Expr(field);               }
        public IExpr<T1, T2> SubQuery(ISelectQuery selectQuery) { return Expr(selectQuery);            }
        public IExpr<T1, T2> Value   (object         value)       { return Expr(new SqlValue(value)); }

        public T2 Exists(ISelectQuery subQuery)
        {
            _condition.Search.Conditions.Add(new Condition(true, new FuncLike(SqlFunction.CreateExists(subQuery))));
            return _condition.GetNext();
        }
    }
}