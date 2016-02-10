namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class HierarhicalLike : Like,
                                   IHierarhicalLike
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
                                                 (IQueryExpression)Expr1.Clone(objectTree, doClone), (IQueryExpression)Expr2.Clone(objectTree, doClone), _start, _end));

            return clone;
        }

        public override EQueryElementType ElementType => EQueryElementType.LikePredicate;

        public HierarhicalLike(IQueryExpression exp1, IQueryExpression exp2, string start, string end)
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