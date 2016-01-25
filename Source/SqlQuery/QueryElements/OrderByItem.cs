namespace LinqToDB.SqlQuery.QueryElements
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class OrderByItem : BaseQueryElement, ICloneableElement
    {
        public OrderByItem(ISqlExpression expression, bool isDescending)
        {
            Expression   = expression;
            IsDescending = isDescending;
        }

        public ISqlExpression Expression   { get; internal set; }
        public bool           IsDescending { get; private set; }

        internal void Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            Expression = Expression.Walk(skipColumns, func);
        }

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new OrderByItem((ISqlExpression)Expression.Clone(objectTree, doClone), IsDescending));

            return clone;
        }

        #region Overrides

#if OVERRIDETOSTRING

			public override string ToString()
			{
				return ((IQueryElement)this).ToString(new StringBuilder(), new Dictionary<IQueryElement,IQueryElement>()).ToString();
			}

#endif

        #endregion

        #region IQueryElement Members

        protected override IEnumerable<IQueryElement> GetChildItemsInternal()
        {
            yield return Expression;
        }

        public override QueryElementType ElementType => QueryElementType.OrderByItem;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            Expression.ToString(sb, dic);

            if (IsDescending)
                sb.Append(" DESC");

            return sb;
        }

        #endregion
    }
}