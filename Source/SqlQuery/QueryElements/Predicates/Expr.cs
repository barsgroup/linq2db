namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class Expr : Predicate
    {
        public Expr([JetBrains.Annotations.NotNull] ISqlExpression exp1, int precedence)
            : base(precedence)
        {
            if (exp1 == null) throw new ArgumentNullException("exp1");

            Expr1 = exp1;
        }

        public Expr([JetBrains.Annotations.NotNull] ISqlExpression exp1)
            : base(exp1.Precedence)
        {
            if (exp1 == null) throw new ArgumentNullException("exp1");

            Expr1 = exp1;
        }

        public ISqlExpression Expr1 { get; set; }

        protected override void Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            Expr1 = Expr1.Walk(skipColumns, func);

            if (Expr1 == null)
                throw new InvalidOperationException();
        }

        public override bool CanBeNull()
        {
            return Expr1.CanBeNull();
        }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement,ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new Expr((ISqlExpression)Expr1.Clone(objectTree, doClone), Precedence));

            return clone;
        }

        protected override IEnumerable<IQueryElement> GetChildItemsInternal()
        {
            yield return Expr1;
        }

        public override QueryElementType ElementType => QueryElementType.ExprPredicate;

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            Expr1.ToString(sb, dic);
        }
    }
}