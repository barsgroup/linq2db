namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class NotExpr : Expr
    {
        public NotExpr(ISqlExpression exp1, bool isNot, int precedence)
            : base(exp1, precedence)
        {
            IsNot = isNot;
        }

        public bool IsNot { get; set; }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement,ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new NotExpr((ISqlExpression)Expr1.Clone(objectTree, doClone), IsNot, Precedence));

            return clone;
        }

        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
            list.Add(Expr1);
        }

        public override QueryElementType ElementType => QueryElementType.NotExprPredicate;

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            if (IsNot) sb.Append("NOT (");
            base.ToString(sb, dic);
            if (IsNot) sb.Append(")");
        }
    }
}