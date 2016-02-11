namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class IsNull : NotExpr,
                          IIsNull
    {
        public IsNull(IQueryExpression exp1, bool isNot)
            : base(exp1, isNot, SqlQuery.Precedence.Comparison)
        {
        }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement,ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new IsNull((IQueryExpression)Expr1.Clone(objectTree, doClone), IsNot));

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

        public override void GetChildren(LinkedList<IQueryElement> list)
        {
            list.AddLast(Expr1);
        }

        public override EQueryElementType ElementType => EQueryElementType.IsNullPredicate;
    }
}