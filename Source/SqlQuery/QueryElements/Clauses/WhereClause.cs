namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class WhereClause : ClauseBase<WhereClause,WhereClause.Next>, IQueryElement, ISqlExpressionWalkable
    {
        public class Next : ClauseBase
        {
            internal Next(WhereClause parent) : base(parent.SelectQuery)
            {
                _parent = parent;
            }

            readonly WhereClause _parent;

            public WhereClause Or => _parent.SetOr(true);

            public WhereClause And => _parent.SetOr(false);

            protected override void GetChildrenInternal(List<IQueryElement> list)
            {
            }
        }

        internal WhereClause(ISelectQuery selectQuery) : base(selectQuery)
        {
            SearchCondition = new SearchCondition();
        }

        internal WhereClause(
            ISelectQuery selectQuery,
            WhereClause clone,
            Dictionary<ICloneableElement,ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
            : base(selectQuery)
        {
            SearchCondition = (SearchCondition)clone.SearchCondition.Clone(objectTree, doClone);
        }

        internal WhereClause(SearchCondition searchCondition) : base(null)
        {
            SearchCondition = searchCondition;
        }

        public SearchCondition SearchCondition { get; private set; }

        public bool IsEmpty => SearchCondition.Conditions.Count == 0;

        protected override SearchCondition Search => SearchCondition;

        protected override Next GetNext()
        {
            return new Next(this);
        }

        #region ISqlExpressionWalkable Members

        ISqlExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> action)
        {
            SearchCondition = (SearchCondition)((ISqlExpressionWalkable)SearchCondition).Walk(skipColumns, action);
            return null;
        }

        #endregion

        #region IQueryElement Members

        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
            list.Add(SearchCondition);
        }

        public override QueryElementType ElementType => QueryElementType.WhereClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            if (Search.Conditions.Count == 0)
                return sb;

            sb.Append("\nWHERE\n\t");
            return ((IQueryElement)Search).ToString(sb, dic);
        }

        #endregion
    }
}