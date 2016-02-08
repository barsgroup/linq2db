namespace LinqToDB.SqlQuery.QueryElements
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class JoinedTable : BaseQueryElement, ISqlExpressionWalkable, ICloneableElement
    {
        public JoinedTable(JoinType joinType, ITableSource table, bool isWeak, SearchCondition searchCondition)
        {
            JoinType        = joinType;
            Table           = table;
            IsWeak          = isWeak;
            Condition       = searchCondition;
            CanConvertApply = true;
        }

        public JoinedTable(JoinType joinType, ITableSource table, bool isWeak)
            : this(joinType, table, isWeak, new SearchCondition())
        {
        }

        public JoinedTable(JoinType joinType, ISqlTableSource table, string alias, bool isWeak)
            : this(joinType, new TableSource(table, alias), isWeak)
        {
        }

        public JoinType        JoinType        { get; set; }
        public ITableSource Table           { get; set; }
        public SearchCondition Condition       { get; private set; }
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
                                                 (SearchCondition)Condition.Clone(objectTree, doClone)));

            return clone;
        }

#if OVERRIDETOSTRING

			public override string ToString()
			{
				return ((IQueryElement)this).ToString(new StringBuilder(), new Dictionary<IQueryElement,IQueryElement>()).ToString();
			}

#endif

        #region ISqlExpressionWalkable Members

        public ISqlExpression Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> action)
        {
            Condition = (SearchCondition)((ISqlExpressionWalkable)Condition).Walk(skipColumns, action);

            Table.Walk(skipColumns, action);

            return null;
        }

        #endregion

        #region IQueryElement Members

        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
            list.Add(Table);
            list.Add(Condition);
        }

        public override QueryElementType ElementType => QueryElementType.JoinedTable;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            if (dic.ContainsKey(this))
                return sb.Append("...");

            dic.Add(this, this);

            switch (JoinType)
            {
                case JoinType.Inner      : sb.Append("INNER JOIN ");  break;
                case JoinType.Left       : sb.Append("LEFT JOIN ");   break;
                case JoinType.CrossApply : sb.Append("CROSS APPLY "); break;
                case JoinType.OuterApply : sb.Append("OUTER APPLY "); break;
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