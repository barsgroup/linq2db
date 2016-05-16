using System;
using System.Collections.Generic;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Clauses.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Conditions;
using Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Clauses
{
    public interface IWhereClause : IClauseBase, ISqlExpressionWalkable, IConditionBase<IWhereClause, WhereClause.Next>
    {
        [SearchContainer]
        ISearchCondition Search { get; }
    }

    public class WhereClause : ClauseBase<IWhereClause, WhereClause.Next>,
        IWhereClause
    {
        internal WhereClause(ISelectQuery selectQuery) : base(selectQuery)
        {
            Search = new SearchCondition();
        }

        internal WhereClause(
            ISelectQuery selectQuery,
            IWhereClause clone,
            Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
            : base(selectQuery)
        {
            Search = (ISearchCondition) clone.Search.Clone(objectTree, doClone);
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

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> action)
        {
            Search = (ISearchCondition) Search.Walk(skipColumns, action);
            return null;
        }

        #endregion

        public class Next : ClauseBase
        {
            private readonly IWhereClause _parent;

            internal Next(IWhereClause parent) : base(parent.SelectQuery)
            {
                _parent = parent;
            }
        }

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.WhereClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            if (Search.Conditions.Count == 0)
                return sb;

            sb.Append("\nWHERE\n\t");
            return Search.ToString(sb, dic);
        }

        #endregion
    }
}