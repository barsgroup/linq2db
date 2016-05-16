using System.Collections.Generic;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements
{
    public class Union : BaseQueryElement,
        IUnion
    {
        public Union(ISelectQuery selectQuery, bool isAll)
        {
            SelectQuery = selectQuery;
            IsAll = isAll;
        }

        public ISelectQuery SelectQuery { get; }

        public bool IsAll { get; }

        public override EQueryElementType ElementType => EQueryElementType.Union;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            sb.Append(" \nUNION").Append(IsAll ? " ALL" : "").Append(" \n");
            return SelectQuery.ToString(sb, dic);
        }

#if OVERRIDETOSTRING

        public override string ToString()
        {
            return
                ((IQueryElement) this).ToString(new StringBuilder(), new Dictionary<IQueryElement, IQueryElement>())
                    .ToString();
        }

#endif
    }
}