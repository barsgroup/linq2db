namespace LinqToDB.SqlQuery.QueryElements
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class OrderByItem : BaseQueryElement,
                               IOrderByItem
    {
        public OrderByItem(ISqlExpression expression, bool isDescending)
        {
            Expression   = expression;
            IsDescending = isDescending;
        }

        public ISqlExpression Expression   { get;  set; }
        public bool           IsDescending { get; private set; }

        public ISqlExpression Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            Expression = Expression.Walk(skipColumns, func);

            return null;
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

        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
            list.Add(Expression);
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