namespace LinqToDB.SqlQuery.QueryElements
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    [DebuggerDisplay("SQL = {SqlText}")]
    public abstract class BaseQueryElement : IQueryElement
    {
        public abstract EQueryElementType ElementType { get; }

        public abstract StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic);

        public string SqlText => ToString(new StringBuilder(), new Dictionary<IQueryElement, IQueryElement>()).ToString();
    }
}