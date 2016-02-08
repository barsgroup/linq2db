namespace LinqToDB.SqlQuery.QueryElements.Conditions
{
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class Join : ConditionBase<IJoin, Join.Next>,
                        IJoin
    {
        public class Next
        {
            internal Next(IJoin parent)
            {
                _parent = parent;
            }

            readonly IJoin _parent;

            public IJoin Or => _parent.SetOr(true);

            public IJoin And => _parent.SetOr(false);

            public static implicit operator Join(Next next)
            {
                return (Join)next._parent;
            }
        }

        public override ISearchCondition Search => JoinedTable.Condition;

        public override Next GetNext()
        {
            return new Next(this);
        }

        internal Join(JoinType joinType, ISqlTableSource table, string alias, bool isWeak, ICollection<IJoin> joins)
        {
            JoinedTable = new JoinedTable(joinType, table, alias, isWeak);

            if (joins != null && joins.Count > 0)
                foreach (var join in joins)
                    JoinedTable.Table.Joins.Add(@join.JoinedTable);
        }

        public IJoinedTable JoinedTable { get; }

        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
        }

        public override EQueryElementType ElementType => EQueryElementType.None;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            return sb;
        }
    }
}