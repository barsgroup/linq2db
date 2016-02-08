namespace LinqToDB.SqlQuery.QueryElements
{
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public class Union : BaseQueryElement,
                         IUnion
    {
        public Union(ISelectQuery selectQuery, bool isAll)
        {
            SelectQuery = selectQuery;
            IsAll    = isAll;
        }

        public ISelectQuery SelectQuery { get; private set; }
        public bool IsAll { get; private set; }

        public override EQueryElementType ElementType => EQueryElementType.Union;

#if OVERRIDETOSTRING

			public override string ToString()
			{
				return ((IQueryElement)this).ToString(new StringBuilder(), new Dictionary<IQueryElement,IQueryElement>()).ToString();
			}

#endif

        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
            list.Add(SelectQuery);
        }

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            sb.Append(" \nUNION").Append(IsAll ? " ALL" : "").Append(" \n");
            return SelectQuery.ToString(sb, dic);
        }
    }
}