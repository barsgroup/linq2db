namespace LinqToDB.SqlQuery.QueryElements.Conditions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ISearchCondition : ISqlExpression,
        IConditionBase<ISearchCondition, SearchCondition.NextCondition>, ISqlPredicate
    {
        List<ICondition> Conditions { get; }
    }

    public class SearchCondition : ConditionBase<ISearchCondition, SearchCondition.NextCondition>,
                                   ISearchCondition
    {
        public SearchCondition()
        {
        }

        public SearchCondition(IEnumerable<ICondition> list)
        {
            _conditions.AddRange(list);
        }

        public SearchCondition(params ICondition[] list)
        {
            _conditions.AddRange(list);
        }

        readonly List<ICondition> _conditions = new List<ICondition>();
        public   List<ICondition>  Conditions => _conditions;

        public override ISearchCondition Search => this;

        public override NextCondition GetNext()
        {
            return new NextCondition(this);
        }

        #region Overrides

#if OVERRIDETOSTRING

			public override string ToString()
			{
				return ((IQueryElement)this).ToString(new StringBuilder(), new Dictionary<IQueryElement,IQueryElement>()).ToString();
			}

#endif

        #endregion

        #region IPredicate Members

        public int Precedence
        {
            get
            {
                if (_conditions.Count == 0) return SqlQuery.Precedence.Unknown;
                if (_conditions.Count == 1) return _conditions[0].Precedence;

                return _conditions.Select(_ =>
                                          _.IsNot ? SqlQuery.Precedence.LogicalNegation :
                                              _.IsOr  ? SqlQuery.Precedence.LogicalDisjunction :
                                                  SqlQuery.Precedence.LogicalConjunction).Min();
            }
        }

        public Type SystemType => typeof(bool);

        ISqlExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            foreach (var condition in Conditions)
                condition.Predicate.Walk(skipColumns, func);

            return func(this);
        }

        #endregion

        #region IEquatable<ISqlExpression> Members

        bool IEquatable<ISqlExpression>.Equals(ISqlExpression other)
        {
            return this == other;
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

        public bool Equals(ISqlExpression other, Func<ISqlExpression,ISqlExpression,bool> comparer)
        {
            return this == other;
        }

        #endregion

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
            {
                var sc = new SearchCondition();

                objectTree.Add(this, clone = sc);

                sc._conditions.AddRange(_conditions.Select(c => (ICondition)c.Clone(objectTree, doClone)));
            }

            return clone;
        }

        #endregion

        #region IQueryElement Members

        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
            list.AddRange(Conditions);
        }

        public override EQueryElementType ElementType => EQueryElementType.SearchCondition;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            if (dic.ContainsKey(this))
                return sb.Append("...");

            dic.Add(this, this);

            foreach (IQueryElement c in Conditions)
                c.ToString(sb, dic);

            if (Conditions.Count > 0)
                sb.Length -= 4;

            dic.Remove(this);

            return sb;
        }

        #endregion

        public class NextCondition
        {
            internal NextCondition(ISearchCondition parent)
            {
                _parent = parent;
            }

            readonly ISearchCondition _parent;

            public ISearchCondition Or => _parent.SetOr(true);

            public ISearchCondition And => _parent.SetOr(false);

            public ISqlExpression ToExpr() { return _parent; }
        }
    }
}