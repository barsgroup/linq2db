using System;
using System.Collections.Generic;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Predicates
{
    public class InSubQuery : NotExpr,
        IInSubQuery
    {
        public InSubQuery(IQueryExpression exp1, bool isNot, ISelectQuery subQuery)
            : base(exp1, isNot, SqlQuery.Precedence.Comparison)
        {
            SubQuery = subQuery;
        }

        public ISelectQuery SubQuery { get; private set; }

        public override EQueryElementType ElementType => EQueryElementType.InSubQueryPredicate;

        protected override void Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            base.Walk(skipColumns, func);
            SubQuery = (ISelectQuery) SubQuery.Walk(skipColumns, func);
        }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new InSubQuery(
                    (IQueryExpression) Expr1.Clone(objectTree, doClone),
                    IsNot,
                    (ISelectQuery) SubQuery.Clone(objectTree, doClone)));

            return clone;
        }

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            Expr1.ToString(sb, dic);

            if (IsNot) sb.Append(" NOT");
            sb.Append(" IN (");

            SubQuery.ToString(sb, dic);
            sb.Append(")");
        }
    }
}