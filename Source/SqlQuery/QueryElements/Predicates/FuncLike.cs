using System;
using System.Collections.Generic;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Predicates
{
    public class FuncLike : Predicate,
        IFuncLike
    {
        public FuncLike(ISqlFunction func)
            : base(func.Precedence)
        {
            Function = func;
        }

        public ISqlFunction Function { get; private set; }

        public override bool CanBeNull()
        {
            return Function.CanBeNull();
        }

        public override EQueryElementType ElementType => EQueryElementType.FuncLikePredicate;

        protected override void Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            Function = (ISqlFunction) Function.Walk(skipColumns, func);
        }

        protected override ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new FuncLike((ISqlFunction) Function.Clone(objectTree, doClone)));

            return clone;
        }

        protected override void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            Function.ToString(sb, dic);
        }
    }
}