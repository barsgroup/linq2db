namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class NotExpr : Expr,
                           INotExpr
    {
        public NotExpr(IQueryExpression exp1, bool isNot, int precedence)
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
                objectTree.Add(this, clone = new NotExpr((IQueryExpression)Expr1.Clone(objectTree, doClone), IsNot, Precedence));

            return clone;
        }

        public override void GetChildren(LinkedList<IQueryElement> list)
        {
            list.AddLast(Expr1);
        }

        public override EQueryElementType ElementType => EQueryElementType.NotExprPredicate;

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            if (IsNot) sb.Append("NOT (");
            base.ToString(sb, dic);
            if (IsNot) sb.Append(")");
        }
    }
}