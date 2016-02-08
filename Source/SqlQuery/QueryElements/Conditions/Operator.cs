namespace LinqToDB.SqlQuery.QueryElements.Conditions
{
    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class Operator<T1, T2> : IOperator<T2>
        where T1 : IConditionBase<T1,T2>
    {
        internal Operator(IExpr<T1, T2> expr, EOperator op) 
        {
            _expr = expr;
            _op   = op;
        }

        readonly IExpr<T1, T2> _expr;
        readonly EOperator _op;

        public T2 Expr    (ISqlExpression expr)       { return _expr.Add(new ExprExpr(_expr.SqlExpression, _op, expr)); }
        public T2 Field   (SqlField      field)       { return Expr(field);               }
        public T2 SubQuery(ISelectQuery selectQuery) { return Expr(selectQuery);         }
        public T2 Value   (object        value)       { return Expr(new SqlValue(value)); }

        public T2 All     (ISelectQuery subQuery)    { return Expr(SqlFunction.CreateAll (subQuery)); }
        public T2 Some    (ISelectQuery subQuery)    { return Expr(SqlFunction.CreateSome(subQuery)); }
        public T2 Any     (ISelectQuery subQuery)    { return Expr(SqlFunction.CreateAny (subQuery)); }
    }
}