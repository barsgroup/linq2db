namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class InSubQuery : NotExpr
    {
        public InSubQuery(ISqlExpression exp1, bool isNot, SelectQuery subQuery)
            : base(exp1, isNot, SqlQuery.Precedence.Comparison)
        {
            SubQuery = subQuery;
        }

        public SelectQuery SubQuery { get; private set; }

        protected override void Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            base.Walk(skipColumns, func);
            SubQuery = (SelectQuery)((ISqlExpression)SubQuery).Walk(skipColumns, func);
        }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement,ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new InSubQuery(
                                                 (ISqlExpression)Expr1.Clone(objectTree, doClone),
                                                 IsNot,
                                                 (SelectQuery)SubQuery.Clone(objectTree, doClone)));

            return clone;
        }
        protected override IEnumerable<IQueryElement> GetChildItemsInternal()
        {
            return base.GetChildItemsInternal().UnionChilds(Expr1).UnionChilds(SubQuery);
        }

        public override QueryElementType ElementType => QueryElementType.InSubQueryPredicate;

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            Expr1.ToString(sb, dic);

            if (IsNot) sb.Append(" NOT");
            sb.Append(" IN (");

            ((IQueryElement)SubQuery).ToString(sb, dic);
            sb.Append(")");
        }
    }
}