using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bars2Db.Extensions;
using Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Conditions
{
    public class SearchCondition : ConditionBase<ISearchCondition, SearchCondition.NextCondition>,
        ISearchCondition
    {
        public SearchCondition()
        {
        }

        public SearchCondition(LinkedList<ICondition> list)
        {
            list.ForEach(node => Conditions.AddLast(node.Value));
        }

        public SearchCondition(params ICondition[] list)
        {
            for (var i = 0; i < list.Length; i++)
            {
                Conditions.AddLast(list[i]);
            }
        }

        public LinkedList<ICondition> Conditions { get; } = new LinkedList<ICondition>();

        public override ISearchCondition Search
        {
            get { return this; }
            protected set { throw new NotSupportedException(); }
        }

        public override NextCondition GetNext()
        {
            return new NextCondition(this);
        }

        #region IEquatable<ISqlExpression> Members

        bool IEquatable<IQueryExpression>.Equals(IQueryExpression other)
        {
            return this == other;
        }

        #endregion

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
            {
                var sc = new SearchCondition();

                objectTree.Add(this, clone = sc);

                Conditions.ForEach(node => sc.Conditions.AddLast((ICondition) node.Value.Clone(objectTree, doClone)));
            }

            return clone;
        }

        #endregion

        #region Overrides

#if OVERRIDETOSTRING

        public override string ToString()
        {
            return
                ((IQueryElement) this).ToString(new StringBuilder(), new Dictionary<IQueryElement, IQueryElement>())
                    .ToString();
        }

#endif

        #endregion

        public class NextCondition
        {
            private readonly ISearchCondition _parent;

            internal NextCondition(ISearchCondition parent)
            {
                _parent = parent;
            }

            public ISearchCondition Or => _parent.SetOr(true);

            public ISearchCondition And => _parent.SetOr(false);

            public IQueryExpression ToExpr()
            {
                return _parent;
            }
        }

        #region IPredicate Members

        public int Precedence
        {
            get
            {
                if (Conditions.Count == 0) return SqlQuery.Precedence.Unknown;
                if (Conditions.Count == 1) return Conditions.First.Value.Precedence;

                return Conditions.Select(_ =>
                    _.IsNot
                        ? SqlQuery.Precedence.LogicalNegation
                        : _.IsOr
                            ? SqlQuery.Precedence.LogicalDisjunction
                            : SqlQuery.Precedence.LogicalConjunction).Min();
            }
        }

        public Type SystemType => typeof(bool);

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            foreach (var condition in Conditions)
                condition.Predicate.Walk(skipColumns, func);

            return func(this);
        }

        #endregion

        #region ISqlExpression Members

        public bool CanBeNull()
        {
            foreach (var c in Conditions)
                if (c.CanBeNull())
                    return true;

            return false;
        }

        public bool Equals(IQueryExpression other, Func<IQueryExpression, IQueryExpression, bool> comparer)
        {
            return this == other;
        }

        #endregion

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.SearchCondition;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            if (dic.ContainsKey(this))
                return sb.Append("...");

            dic.Add(this, this);

            foreach (var c in Conditions)
                c.ToString(sb, dic);

            if (Conditions.Count > 0)
                sb.Length -= 4;

            dic.Remove(this);

            return sb;
        }

        #endregion
    }
}