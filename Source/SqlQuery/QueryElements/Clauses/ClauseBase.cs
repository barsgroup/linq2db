namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public abstract class ClauseBase : BaseQueryElement,
                                       IClauseBase
    {
        protected ClauseBase(ISelectQuery selectQuery)
        {
            SelectQuery = selectQuery;
        }

        public ISelectClause Select => SelectQuery.Select;

        public IFromClause From => SelectQuery.From;

        public WhereClause Where => SelectQuery.Where;

        public GroupByClause GroupBy => SelectQuery.GroupBy;

        public WhereClause Having => SelectQuery.Having;

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

    public abstract class ClauseBase<T1, T2> : ConditionBase<T1, T2>
        where T1 : ClauseBase<T1, T2>
    {
        protected ClauseBase(ISelectQuery selectQuery)
        {
            SelectQuery = selectQuery;
        }

        public ISelectClause Select => SelectQuery.Select;

        public IFromClause From => SelectQuery.From;

        public GroupByClause GroupBy => SelectQuery.GroupBy;

        public WhereClause   Having => SelectQuery.Having;

        public IOrderByClause OrderBy => SelectQuery.OrderBy;

        public ISelectQuery End()   { return SelectQuery; }

        protected internal ISelectQuery SelectQuery { get; private set; }

        internal void SetSqlQuery(ISelectQuery selectQuery)
        {
            SelectQuery = selectQuery;
        }
    }
}