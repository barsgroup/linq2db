namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class IsNull : NotExpr
    {
        public IsNull(ISqlExpression exp1, bool isNot)
            : base(exp1, isNot, SqlQuery.Precedence.Comparison)
        {
        }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement,ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new IsNull((ISqlExpression)Expr1.Clone(objectTree, doClone), IsNot));

            return clone;
        }

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            Expr1.ToString(sb, dic);
            sb
                .Append(" IS ")
                .Append(IsNot ? "NOT " : "")
                .Append("NULL");
        }

        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
            list.Add(Expr1);
        }

        public override QueryElementType ElementType => QueryElementType.IsNullPredicate;
    }
}