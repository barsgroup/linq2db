namespace LinqToDB.SqlQuery.QueryElements.Conditions
{
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class Join : ConditionBase<Join,Join.Next>
    {
        public class Next
        {
            internal Next(Join parent)
            {
                _parent = parent;
            }

            readonly Join _parent;

            public Join Or => _parent.SetOr(true);

            public Join And => _parent.SetOr(false);

            public static implicit operator Join(Next next)
            {
                return next._parent;
            }
        }

        protected override SearchCondition Search => JoinedTable.Condition;

        protected override Next GetNext()
        {
            return new Next(this);
        }

        internal Join(JoinType joinType, ISqlTableSource table, string alias, bool isWeak, ICollection<Join> joins)
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