namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.Properties;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public class Expr : Predicate,
                        IExpr
    {
        public Expr([NotNull] IQueryExpression exp1, int precedence)
            : base(precedence)
        {
            if (exp1 == null) throw new ArgumentNullException(nameof(exp1));

            Expr1 = exp1;
        }

        public Expr([NotNull] IQueryExpression exp1)
            : base(exp1.Precedence)
        {
            if (exp1 == null) throw new ArgumentNullException(nameof(exp1));

            Expr1 = exp1;
        }

        public IQueryExpression Expr1 { get; set; }

        protected override void Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> func)
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
                objectTree.Add(this, clone = new Expr((IQueryExpression)Expr1.Clone(objectTree, doClone), Precedence));

            return clone;
        }

        public override EQueryElementType ElementType => EQueryElementType.ExprPredicate;

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            Expr1.ToString(sb, dic);
        }
    }
}