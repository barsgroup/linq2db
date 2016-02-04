namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.SqlElements;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class FuncLike : Predicate
    {
        public FuncLike(SqlFunction func)
            : base(func.Precedence)
        {
            Function = func;
        }

        public SqlFunction Function { get; private set; }

        protected override void Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            Function = (SqlFunction)((ISqlExpression)Function).Walk(skipColumns, func);
        }

        public override bool CanBeNull()
        {
            return Function.CanBeNull();
        }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement,ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new FuncLike((SqlFunction)Function.Clone(objectTree, doClone)));

            return clone;
        }

        public override QueryElementType ElementType => QueryElementType.FuncLikePredicate;

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            ((IQueryElement)Function).ToString(sb, dic);
        }
        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
            list.Add(Function);
        }

    }
}