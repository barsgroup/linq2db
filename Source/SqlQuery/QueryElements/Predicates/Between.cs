namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class Between : NotExpr
    {
        public Between(ISqlExpression exp1, bool isNot, ISqlExpression exp2, ISqlExpression exp3)
            : base(exp1, isNot, SqlQuery.Precedence.Comparison)
        {
            Expr2 = exp2;
            Expr3 = exp3;
        }

        public ISqlExpression Expr2 { get; internal set; }
        public ISqlExpression Expr3 { get; internal set; }

        protected override void Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            base.Walk(skipColumns, func);
            Expr2 = Expr2.Walk(skipColumns, func);
            Expr3 = Expr3.Walk(skipColumns, func);
        }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement,ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new Between(
                                                 (ISqlExpression)Expr1.Clone(objectTree, doClone),
                                                 IsNot,
                                                 (ISqlExpression)Expr2.Clone(objectTree, doClone),
                                                 (ISqlExpression)Expr3.Clone(objectTree, doClone)));

            return clone;
        }

        protected override IEnumerable<IQueryElement> GetChildItemsInternal()
        {
            return base.GetChildItemsInternal().UnionChilds(Expr1).UnionChilds(Expr2).UnionChilds(Expr3);
        }
        public override QueryElementType ElementType => QueryElementType.BetweenPredicate;

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