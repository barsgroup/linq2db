namespace LinqToDB.SqlQuery.QueryElements
{
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public class Union : BaseQueryElement, IQueryElement
    {
        public Union()
        {
        }

        public Union(SelectQuery selectQuery, bool isAll)
        {
            SelectQuery = selectQuery;
            IsAll    = isAll;
        }

        public SelectQuery SelectQuery { get; private set; }
        public bool IsAll { get; private set; }

        public override QueryElementType ElementType => QueryElementType.Union;

#if OVERRIDETOSTRING

			public override string ToString()
			{
				return ((IQueryElement)this).ToString(new StringBuilder(), new Dictionary<IQueryElement,IQueryElement>()).ToString();
			}

#endif

        protected override IEnumerable<IQueryElement> GetChildItemsInternal()
        {
            var resultItems =  base.GetChildItemsInternal();
            return resultItems.UnionChilds(SelectQuery);
        }

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            sb.Append(" \nUNION").Append(IsAll ? " ALL" : "").Append(" \n");
            return ((IQueryElement)SelectQuery).ToString(sb, dic);
        }
    }
}