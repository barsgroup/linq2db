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

        public T2 Expr    (IQueryExpression expr)       { return _expr.Add(new ExprExpr(_expr.SqlExpression, _op, expr)); }
        public T2 Field   (ISqlField field)       { return Expr(field);               }
    }
}