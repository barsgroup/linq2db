using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Predicates
{
    public class InList : NotExpr,
        IInList
    {
        public InList(IQueryExpression exp1, bool isNot, params IQueryExpression[] values)
            : base(exp1, isNot, SqlQuery.Precedence.Comparison)
        {
            if (values != null && values.Length > 0)
                Values.AddRange(values);
        }

        public InList(IQueryExpression exp1, bool isNot, IEnumerable<IQueryExpression> values)
            : base(exp1, isNot, SqlQuery.Precedence.Comparison)
        {
            if (values != null)
                Values.AddRange(values);
        }

        public List<IQueryExpression> Values { get; } = new List<IQueryExpression>();

        public override EQueryElementType ElementType => EQueryElementType.InListPredicate;

        protected override void Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> action)
        {
            base.Walk(skipColumns, action);
            for (var i = 0; i < Values.Count; i++)
                Values[i] = Values[i].Walk(skipColumns, action);
        }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
            {
                objectTree.Add(this, clone = new InList(
                    (IQueryExpression) Expr1.Clone(objectTree, doClone),
                    IsNot,
                    Values.Select(e => (IQueryExpression) e.Clone(objectTree, doClone)).ToArray()));
            }

            return clone;
        }

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            Expr1.ToString(sb, dic);

            if (IsNot) sb.Append(" NOT");
            sb.Append(" IN (");

            foreach (var value in Values)
            {
                value.ToString(sb, dic);
                sb.Append(',');
            }

            if (Values.Count > 0)
                sb.Length--;

            sb.Append(")");
        }
    }
}