namespace LinqToDB.SqlQuery.QueryElements.Conditions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class SearchCondition : ConditionBase<SearchCondition, NextCondition>, ISqlPredicate, ISqlExpression
    {
        public SearchCondition()
        {
        }

        public SearchCondition(IEnumerable<Condition> list)
        {
            _conditions.AddRange(list);
        }

        public SearchCondition(params Condition[] list)
        {
            _conditions.AddRange(list);
        }

        readonly List<Condition> _conditions = new List<Condition>();
        public   List<Condition>  Conditions => _conditions;

        protected override SearchCondition Search => this;

        protected override NextCondition GetNext()
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

                sc._conditions.AddRange(_conditions.Select(c => (Condition)c.Clone(objectTree, doClone)));
            }

            return clone;
        }

        #endregion

        #region IQueryElement Members

        protected override IEnumerable<IQueryElement> GetChildItemsInternal()
        {
            return base.GetChildItemsInternal().UnionChilds(Conditions);
        }

        public override QueryElementType ElementType => QueryElementType.SearchCondition;

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
    }
}