using System;
using System.Collections.Generic;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Predicates
{
    public enum HierarhicalFlow
    {
        IsChildOf,

        IsParentOf,

        Contains
    }

    public class HierarhicalPredicate : Expr,
        IHierarhicalPredicate
    {
        public HierarhicalPredicate(IQueryExpression exp1, IQueryExpression exp2, HierarhicalFlow flow) : base(exp1)
        {
            Flow = flow;
            Expr2 = exp2;
        }

        public HierarhicalFlow Flow { get; }

        public IQueryExpression Expr2 { get; set; }

        public override EQueryElementType ElementType => EQueryElementType.HierarhicalPredicate;

        public string GetOperator()
        {
            switch (Flow)
            {
                case HierarhicalFlow.IsChildOf:
                    return "<";
                case HierarhicalFlow.IsParentOf:
                    return ">";
                case HierarhicalFlow.Contains:
                    return "=";
            }

            throw new NotSupportedException();
        }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this,
                    clone =
                        new HierarhicalPredicate((IQueryExpression) Expr1.Clone(objectTree, doClone),
                            (IQueryExpression) Expr2.Clone(objectTree, doClone), Flow));

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
    }
}