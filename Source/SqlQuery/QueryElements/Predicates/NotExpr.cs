using System;
using System.Collections.Generic;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Predicates
{
    public class NotExpr : Expr,
        INotExpr
    {
        public NotExpr(IQueryExpression exp1, bool isNot, int precedence)
            : base(exp1, precedence)
        {
            IsNot = isNot;
        }

        public bool IsNot { get; set; }

        public override EQueryElementType ElementType => EQueryElementType.NotExprPredicate;

        protected override ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this,
                    clone = new NotExpr((IQueryExpression) Expr1.Clone(objectTree, doClone), IsNot, Precedence));

            return clone;
        }

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            if (IsNot) sb.Append("NOT (");
            ToString(sb, dic);
            if (IsNot) sb.Append(")");
        }
    }
}