using System;
using System.Collections.Generic;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Conditions
{
    public class Join : ConditionBase<IJoin, Join.Next>,
        IJoin
    {
        internal Join(EJoinType joinType, ISqlTableSource table, string alias, bool isWeak, IReadOnlyList<IJoin> joins)
        {
            JoinedTable = new JoinedTable(joinType, table, alias, isWeak);

            if (joins != null && joins.Count > 0)
            {
                for (var index = 0; index < joins.Count; index++)
                {
                    JoinedTable.Table.Joins.AddLast(joins[index].JoinedTable);
                }
            }
        }

        public override ISearchCondition Search
        {
            get { return JoinedTable.Condition; }
            protected set { throw new NotSupportedException(); }
        }

        public override Next GetNext()
        {
            return new Next(this);
        }

        public IJoinedTable JoinedTable { get; }

        public override EQueryElementType ElementType => EQueryElementType.None;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            return sb;
        }

        public class Next
        {
            private readonly IJoin _parent;

            internal Next(IJoin parent)
            {
                _parent = parent;
            }

            public static implicit operator Join(Next next)
            {
                return (Join) next._parent;
            }
        }
    }
}