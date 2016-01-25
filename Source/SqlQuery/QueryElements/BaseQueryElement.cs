namespace LinqToDB.SqlQuery.QueryElements
{
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public abstract class BaseQueryElement : IQueryElement
    {
        public IEnumerable<IQueryElement> GetChildItems()
        {
            return GetChildItemsInternal();
        }

        protected abstract IEnumerable<IQueryElement> GetChildItemsInternal();

        public abstract QueryElementType ElementType { get; }

        public abstract StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic);
    }
}