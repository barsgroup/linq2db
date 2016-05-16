using System;
using System.Collections.Generic;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Predicates
{
    public class IsNull : NotExpr,
        IIsNull
    {
        public IsNull(IQueryExpression exp1, bool isNot)
            : base(exp1, isNot, SqlQuery.Precedence.Comparison)
        {
        }

        public override EQueryElementType ElementType => EQueryElementType.IsNullPredicate;

        protected override ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new IsNull((IQueryExpression) Expr1.Clone(objectTree, doClone), IsNot));

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
    }
}