namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public enum HierarhicalFlow
    {
        IsChildOf,

        IsParentOf,

        Contains
    }

    public class HierarhicalPredicate : Expr,
                                        IHierarhicalPredicate
    {
        public HierarhicalFlow Flow { get; }

        public IQueryExpression Expr2 { get; set; }

        public override EQueryElementType ElementType => EQueryElementType.HierarhicalPredicate;

        public HierarhicalPredicate(IQueryExpression exp1, IQueryExpression exp2, HierarhicalFlow flow) : base(exp1)
        {
            Flow = flow;
            Expr2 = exp2;
        }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new HierarhicalPredicate((IQueryExpression)Expr1.Clone(objectTree, doClone), (IQueryExpression)Expr2.Clone(objectTree, doClone), Flow));

            return clone;
        }

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            Expr1.ToString(sb, dic);

            sb.Append(" " + GetOperator() + " ");

            Expr2.ToString(sb, dic);
        }

        protected override void Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            base.Walk(skipColumns, func);
            Expr2 = Expr2.Walk(skipColumns, func);
        }

        public string GetOperator()
        {
            switch (Flow)
            {
                case HierarhicalFlow.IsChildOf:
                    return "<@";
                case HierarhicalFlow.IsParentOf:
                    return "@>";
                case HierarhicalFlow.Contains:
                    return "@";
            }

            throw new NotSupportedException();
        }
    }
}