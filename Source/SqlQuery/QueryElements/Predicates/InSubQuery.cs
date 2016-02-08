namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class InSubQuery : NotExpr
    {
        public InSubQuery(ISqlExpression exp1, bool isNot, ISelectQuery subQuery)
            : base(exp1, isNot, SqlQuery.Precedence.Comparison)
        {
            SubQuery = subQuery;
        }

        public ISelectQuery SubQuery { get; private set; }

        protected override void Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            base.Walk(skipColumns, func);
            SubQuery = (ISelectQuery)SubQuery.Walk(skipColumns, func);
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
                                                 (ISelectQuery)SubQuery.Clone(objectTree, doClone)));

            return clone;
        }
        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
            list.Add(Expr1);
            list.Add(SubQuery);
        }

        public override EQueryElementType ElementType => EQueryElementType.InSubQueryPredicate;

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