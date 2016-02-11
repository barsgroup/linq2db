namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class InList : NotExpr,
                          IInList
    {
        public InList(IQueryExpression exp1, bool isNot, params IQueryExpression[] values)
            : base(exp1, isNot, SqlQuery.Precedence.Comparison)
        {
            if (values != null && values.Length > 0)
                _values.AddRange(values);
        }

        public InList(IQueryExpression exp1, bool isNot, IEnumerable<IQueryExpression> values)
            : base(exp1, isNot, SqlQuery.Precedence.Comparison)
        {
            if (values != null)
                _values.AddRange(values);
        }

        readonly List<IQueryExpression> _values = new List<IQueryExpression>();
        public   List<IQueryExpression>  Values => _values;

        protected override void Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> action)
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
                                                 (IQueryExpression)Expr1.Clone(objectTree, doClone),
                                                 IsNot,
                                                 _values.Select(e => (IQueryExpression)e.Clone(objectTree, doClone)).ToArray()));
            }

            return clone;
        }

        public override void GetChildren(LinkedList<IQueryElement> list)
        {
            list.AddLast(Expr1);
            FillList(Values, list);
        }

        public override EQueryElementType ElementType => EQueryElementType.InListPredicate;

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