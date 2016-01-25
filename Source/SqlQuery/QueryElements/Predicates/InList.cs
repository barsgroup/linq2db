namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class InList : NotExpr
    {
        public InList(ISqlExpression exp1, bool isNot, params ISqlExpression[] values)
            : base(exp1, isNot, SqlQuery.Precedence.Comparison)
        {
            if (values != null && values.Length > 0)
                _values.AddRange(values);
        }

        public InList(ISqlExpression exp1, bool isNot, IEnumerable<ISqlExpression> values)
            : base(exp1, isNot, SqlQuery.Precedence.Comparison)
        {
            if (values != null)
                _values.AddRange(values);
        }

        readonly List<ISqlExpression> _values = new List<ISqlExpression>();
        public   List<ISqlExpression>  Values => _values;

        protected override void Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> action)
        {
            base.Walk(skipColumns, action);
            for (var i = 0; i < _values.Count; i++)
                _values[i] = _values[i].Walk(skipColumns, action);
        }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement,ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
            {
                objectTree.Add(this, clone = new InList(
                                                 (ISqlExpression)Expr1.Clone(objectTree, doClone),
                                                 IsNot,
                                                 _values.Select(e => (ISqlExpression)e.Clone(objectTree, doClone)).ToArray()));
            }

            return clone;
        }

        protected override IEnumerable<IQueryElement> GetChildItemsInternal()
        {
            return base.GetChildItemsInternal().UnionChilds(Expr1).UnionChilds(Values);
        }

        public override QueryElementType ElementType => QueryElementType.InListPredicate;

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