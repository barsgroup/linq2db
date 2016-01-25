namespace LinqToDB.SqlQuery.QueryElements
{
    using System.Collections.Generic;
    using System.Linq;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public static class QueryElementExtentions
    {
        public static IEnumerable<IQueryElement> UnionChilds(this IEnumerable<IQueryElement> first, IEnumerable<IQueryElement> second)
        {
            return second != null
                       ? first.Union(second.SelectMany(s => s.GetChildItems()))
                       : first;
        }

        public static IEnumerable<IQueryElement> UnionChilds(this IEnumerable<IQueryElement> first, IQueryElement second)
        {
            return second != null
                       ? first.Union(second.GetChildItems())
                       : first;
        }
    }
}