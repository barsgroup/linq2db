namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class ExprExpr : Expr
    {
        public ExprExpr(ISqlExpression exp1, Operator op, ISqlExpression exp2)
            : base(exp1, SqlQuery.Precedence.Comparison)
        {
            Operator = op;
            Expr2    = exp2;
        }

        public new Operator   Operator { get; private  set; }
        public ISqlExpression Expr2    { get; internal set; }

        protected override void Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            base.Walk(skipColumns, func);
            Expr2 = Expr2.Walk(skipColumns, func);
        }

        public override bool CanBeNull()
        {
            return base.CanBeNull() || Expr2.CanBeNull();
        }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement,ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new ExprExpr(
                                                 (ISqlExpression)Expr1.Clone(objectTree, doClone), Operator, (ISqlExpression)Expr2.Clone(objectTree, doClone)));

            return clone;
        }

        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
            list.Add(Expr1);
            list.Add(Expr2);
        }

        public override QueryElementType ElementType => QueryElementType.ExprExprPredicate;

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            Expr1.ToString(sb, dic);

            string op;

            switch (Operator)
            {
                case Operator.Equal         : op = "=";  break;
                case Operator.NotEqual      : op = "<>"; break;
                case Operator.Greater       : op = ">";  break;
                case Operator.GreaterOrEqual: op = ">="; break;
                case Operator.NotGreater    : op = "!>"; break;
                case Operator.Less          : op = "<";  break;
                case Operator.LessOrEqual   : op = "<="; break;
                case Operator.NotLess       : op = "!<"; break;
                default: throw new InvalidOperationException();
            }

            sb.Append(" ").Append(op).Append(" ");

            Expr2.ToString(sb, dic);
        }
    }
}