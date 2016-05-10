using System;
using System.Collections.Generic;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Predicates
{
    public class Between : NotExpr,
        IBetween
    {
        public Between(IQueryExpression exp1, bool isNot, IQueryExpression exp2, IQueryExpression exp3)
            : base(exp1, isNot, SqlQuery.Precedence.Comparison)
        {
            Expr2 = exp2;
            Expr3 = exp3;
        }

        public IQueryExpression Expr2 { get; set; }

        public IQueryExpression Expr3 { get; set; }

        public override EQueryElementType ElementType => EQueryElementType.BetweenPredicate;

        protected override void Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            base.Walk(skipColumns, func);
            Expr2 = Expr2.Walk(skipColumns, func);
            Expr3 = Expr3.Walk(skipColumns, func);
        }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new Between(
                    (IQueryExpression) Expr1.Clone(objectTree, doClone),
                    IsNot,
                    (IQueryExpression) Expr2.Clone(objectTree, doClone),
                    (IQueryExpression) Expr3.Clone(objectTree, doClone)));

            return clone;
        }

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            Expr1.ToString(sb, dic);

            if (IsNot) sb.Append(" NOT");
            sb.Append(" BETWEEN ");

            Expr2.ToString(sb, dic);
            sb.Append(" AND ");
            Expr3.ToString(sb, dic);
        }
    }
}