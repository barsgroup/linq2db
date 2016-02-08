namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class HierarhicalLike : Like
    {
        private readonly string _start;

        private readonly string _end;

        protected override ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new HierarhicalLike(
                                                 (ISqlExpression)Expr1.Clone(objectTree, doClone), (ISqlExpression)Expr2.Clone(objectTree, doClone), _start, _end));

            return clone;
        }

        public override QueryElementType ElementType => QueryElementType.LikePredicate;

        public HierarhicalLike(ISqlExpression exp1, ISqlExpression exp2, string start, string end)
            : base(exp1, false, exp2, null)
        {
            _start = start;
            _end = end;
        }

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            ToString(sb, dic);
        }

        public override string GetOperator()
        {
            if (string.IsNullOrEmpty(_start) && !string.IsNullOrEmpty(_end))
            {
                return "<@";
            }
					
            if (!string.IsNullOrEmpty(_start) && string.IsNullOrEmpty(_end))
            {
                return "@>";
            }
					
            return "@";
        }
    }
}