namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IWhereClause : IClauseBase, ISqlExpressionWalkable, IConditionBase<IWhereClause, WhereClause.Next>
    {
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

            public override void GetChildren(LinkedList<IQueryElement> list)
            {
            }
        }

        internal WhereClause(ISelectQuery selectQuery) : base(selectQuery)
        {
            Search = new SearchCondition();
        }

        internal WhereClause(
            ISelectQuery selectQuery,
            IWhereClause clone,
            Dictionary<ICloneableElement,ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
            : base(selectQuery)
        {
            Search = (ISearchCondition)clone.Search.Clone(objectTree, doClone);
        }

        internal WhereClause(ISearchCondition searchCondition) : base(null)
        {
            Search = searchCondition;
        }

        public override Next GetNext()
        {
            return new Next(this);
        }

        public override ISearchCondition Search { get; protected set; }

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> action)
        {
            Search = (ISearchCondition)Search.Walk(skipColumns, action);
            return null;
        }

        #endregion

        #region IQueryElement Members

        public override void GetChildren(LinkedList<IQueryElement> list)
        {
            list.AddLast(Search);
        }

        public override EQueryElementType ElementType => EQueryElementType.WhereClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            if (Search.Conditions.Count == 0)
                return sb;

            sb.Append("\nWHERE\n\t");
            return Search.ToString(sb, dic);
        }

        #endregion
    }
}