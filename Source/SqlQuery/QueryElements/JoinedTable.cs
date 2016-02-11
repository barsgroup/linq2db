namespace LinqToDB.SqlQuery.QueryElements
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class JoinedTable : BaseQueryElement, IJoinedTable
    {
        public JoinedTable(EJoinType joinType, ITableSource table, bool isWeak, ISearchCondition searchCondition)
        {
            JoinType        = joinType;
            Table           = table;
            IsWeak          = isWeak;
            Condition       = searchCondition;
            CanConvertApply = true;
        }

        public JoinedTable(EJoinType joinType, ITableSource table, bool isWeak)
            : this(joinType, table, isWeak, new SearchCondition())
        {
        }

        public JoinedTable(EJoinType joinType, ISqlTableSource table, string alias, bool isWeak)
            : this(joinType, new TableSource(table, alias), isWeak)
        {
        }

        public EJoinType        JoinType        { get; set; }
        public ITableSource Table           { get; set; }
        public ISearchCondition Condition       { get; private set; }
        public bool            IsWeak          { get; set; }
        public bool            CanConvertApply { get; set; }

        public ICloneableElement Clone(Dictionary<ICloneableElement,ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new JoinedTable(
                                                 JoinType,
                                                 (ITableSource)Table.Clone(objectTree, doClone), 
                                                 IsWeak,
                                                 (ISearchCondition)Condition.Clone(objectTree, doClone)));

            return clone;
        }

#if OVERRIDETOSTRING

			public override string ToString()
			{
				return ((IQueryElement)this).ToString(new StringBuilder(), new Dictionary<IQueryElement,IQueryElement>()).ToString();
			}

#endif

        #region ISqlExpressionWalkable Members

        public IQueryExpression Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> action)
        {
            Condition = (ISearchCondition)((ISqlExpressionWalkable)Condition).Walk(skipColumns, action);

            Table.Walk(skipColumns, action);

            return null;
        }

        #endregion

        #region IQueryElement Members

        public override void GetChildren(LinkedList<IQueryElement> list)
        {
            list.AddLast(Table);
            list.AddLast(Condition);
        }

        public override EQueryElementType ElementType => EQueryElementType.JoinedTable;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            if (dic.ContainsKey(this))
                return sb.Append("...");

            dic.Add(this, this);

            switch (JoinType)
            {
                case EJoinType.Inner      : sb.Append("INNER JOIN ");  break;
                case EJoinType.Left       : sb.Append("LEFT JOIN ");   break;
                case EJoinType.CrossApply : sb.Append("CROSS APPLY "); break;
                case EJoinType.OuterApply : sb.Append("OUTER APPLY "); break;
                default                  : sb.Append("SOME JOIN "); break;
            }

            Table.ToString(sb, dic);
            sb.Append(" ON ");
            ((IQueryElement)Condition).ToString(sb, dic);

            dic.Remove(this);

            return sb;
        }

        #endregion
    }
}