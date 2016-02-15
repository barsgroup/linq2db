namespace LinqToDB.SqlQuery.QueryElements.Predicates
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class FuncLike : Predicate,
                            IFuncLike
    {
        public FuncLike(ISqlFunction func)
            : base(func.Precedence)
        {
            Function = func;
        }

        public ISqlFunction Function { get; private set; }

        protected override void Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> func)
        {
            Function = (ISqlFunction)Function.Walk(skipColumns, func);
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
                objectTree.Add(this, clone = new FuncLike((ISqlFunction)Function.Clone(objectTree, doClone)));

            return clone;
        }

        public override EQueryElementType ElementType => EQueryElementType.FuncLikePredicate;

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            Function.ToString(sb, dic);
        }

        public override void GetChildren(LinkedList<IQueryElement> list)
        {
            list.AddLast(Function);
        }

    }
}