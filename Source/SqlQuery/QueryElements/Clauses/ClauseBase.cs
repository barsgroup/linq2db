namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public abstract class ClauseBase : BaseQueryElement,
                                       IClauseWithConditionBase
    {
        protected ClauseBase(ISelectQuery selectQuery)
        {
            SelectQuery = selectQuery;
        }

        public ISelectClause Select => SelectQuery.Select;

        public IFromClause From => SelectQuery.From;

        public IWhereClause Where => SelectQuery.Where;

        public IGroupByClause GroupBy => SelectQuery.GroupBy;

        public IWhereClause Having => SelectQuery.Having;

        public IOrderByClause OrderBy => SelectQuery.OrderBy;

        public ISelectQuery End() { return SelectQuery; }

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

        public ISelectClause Select => SelectQuery.Select;

        public IFromClause From => SelectQuery.From;

        public IGroupByClause GroupBy => SelectQuery.GroupBy;

        public IWhereClause Having => SelectQuery.Having;

        public IOrderByClause OrderBy => SelectQuery.OrderBy;

        public ISelectQuery End()   { return SelectQuery; }

        public ISelectQuery SelectQuery { get; private set; }

        public void SetSqlQuery(ISelectQuery selectQuery)
        {
            SelectQuery = selectQuery;
        }
    }
}