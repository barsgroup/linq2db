using Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Predicates;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Conditions
{
    public class Operator<T1, T2> : IOperator<T2>
        where T1 : IConditionBase<T1, T2>
    {
        private readonly IExpr<T1, T2> _expr;
        private readonly EOperator _op;

        internal Operator(IExpr<T1, T2> expr, EOperator op)
        {
            _expr = expr;
            _op = op;
        }

        public T2 Expr(IQueryExpression expr)
        {
            return _expr.Add(new ExprExpr(_expr.SqlExpression, _op, expr));
        }

        public T2 Field(ISqlField field)
        {
            return Expr(field);
        }
    }
}