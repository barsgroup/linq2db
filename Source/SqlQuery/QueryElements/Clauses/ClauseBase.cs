using System.Collections.Generic;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Clauses.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Conditions;
using Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Clauses
{
    public abstract class ClauseBase : BaseQueryElement,
        IClauseWithConditionBase
    {
        protected ClauseBase(ISelectQuery selectQuery)
        {
            SelectQuery = selectQuery;
        }

        public IWhereClause Where => SelectQuery.Where;

        public ISelectQuery SelectQuery { get; private set; }

        public void SetSqlQuery(ISelectQuery selectQuery)
        {
            SelectQuery = selectQuery;
        }

        public override EQueryElementType ElementType => EQueryElementType.None;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            return sb;
        }
    }

    public abstract class ClauseBase<T1, T2> : ConditionBase<T1, T2>, IClauseBase
        where T1 : IConditionBase<T1, T2>
    {
        protected ClauseBase(ISelectQuery selectQuery)
        {
            SelectQuery = selectQuery;
        }

        public ISelectQuery SelectQuery { get; private set; }

        public void SetSqlQuery(ISelectQuery selectQuery)
        {
            SelectQuery = selectQuery;
        }
    }
}