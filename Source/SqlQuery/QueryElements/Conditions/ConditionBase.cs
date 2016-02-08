namespace LinqToDB.SqlQuery.QueryElements.Conditions
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using Interfaces;

    public abstract class ConditionBase<T1,T2> : BaseQueryElement,
                                                 IConditionBase<T1, T2>
        where T1 : IConditionBase<T1,T2>
    {
        public abstract ISearchCondition Search { get; }
        public abstract T2              GetNext();

        public T1 SetOr(bool value)
        {
            Search.Conditions[Search.Conditions.Count - 1].IsOr = value;
            return (T1)(IConditionBase<T1, T2>)this;
        }

        public INot<T1, T2> Not => new Not<T1, T2>(this);

        public IExpr<T1, T2> Expr    (ISqlExpression expr)        { return new Expr<T1, T2>(this, false, expr); }
        public IExpr<T1, T2> Field   (SqlField       field)       { return Expr(field);                  }
        public IExpr<T1, T2> SubQuery(ISelectQuery    selectQuery) { return Expr(selectQuery);            }
        public IExpr<T1, T2> Value   (object         value)       { return Expr(new SqlValue(value));    }

        public T2 Exists(ISelectQuery subQuery)
        {
            Search.Conditions.Add(new Condition(false, new FuncLike(SqlFunction.CreateExists(subQuery))));
            return GetNext();
        }

    }
}