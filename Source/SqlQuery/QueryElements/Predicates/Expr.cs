using System;
using System.Collections.Generic;
using System.Text;
using Bars2Db.Properties;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Predicates
{
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

        public override bool CanBeNull()
        {
            return Expr1.CanBeNull();
        }

        public override EQueryElementType ElementType => EQueryElementType.ExprPredicate;

        protected override void Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            Expr1 = Expr1.Walk(skipColumns, func);

            if (Expr1 == null)
                throw new InvalidOperationException();
        }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new Expr((IQueryExpression) Expr1.Clone(objectTree, doClone), Precedence));

            return clone;
        }

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            Expr1.ToString(sb, dic);
        }
    }
}