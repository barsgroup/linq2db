using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements
{
    [DebuggerDisplay("SQL = {SqlText}")]
    public abstract class BaseQueryElement : IQueryElement
    {
        public abstract EQueryElementType ElementType { get; }

        public abstract StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic);

        public string SqlText
            => ToString(new StringBuilder(), new Dictionary<IQueryElement, IQueryElement>()).ToString();
    }
}