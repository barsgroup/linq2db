namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IWhereClause : IClauseBase, ISqlExpressionWalkable, IConditionBase<IWhereClause, WhereClause.Next>
    {
        ISearchCondition SearchCondition { get; }

        bool IsEmpty { get; }
    }

    public class WhereClause : ClauseBase<IWhereClause, WhereClause.Next>,
                               IWhereClause
    {
        public class Next : ClauseBase
        {
            internal Next(IWhereClause parent) : base(parent.SelectQuery)
            {
                _parent = parent;
            }

            readonly IWhereClause _parent;

            public IWhereClause Or => _parent.SetOr(true);

            public IWhereClause And => _parent.SetOr(false);

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
            IWhereClause clone,
            Dictionary<ICloneableElement,ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
            : base(selectQuery)
        {
            SearchCondition = (ISearchCondition)clone.SearchCondition.Clone(objectTree, doClone);
        }

        internal WhereClause(ISearchCondition searchCondition) : base(null)
        {
            SearchCondition = searchCondition;
        }

        public ISearchCondition SearchCondition { get; private set; }

        public bool IsEmpty => SearchCondition.Conditions.Count == 0;

        public override ISearchCondition Search => SearchCondition;

        public override Next GetNext()
        {
            return new Next(this);
        }

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> action)
        {
            SearchCondition = (ISearchCondition)((ISqlExpressionWalkable)SearchCondition).Walk(skipColumns, action);
            return null;
        }

        #endregion

        #region IQueryElement Members

        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
            list.Add(SearchCondition);
        }

        public override EQueryElementType ElementType => EQueryElementType.WhereClause;

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