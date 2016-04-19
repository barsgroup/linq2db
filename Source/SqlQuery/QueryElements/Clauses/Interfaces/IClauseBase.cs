namespace LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Clauses;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public interface IClauseWithConditionBase : IClauseBase
    {
        IWhereClause Where { get; }
    }

    public interface IClauseBase: IQueryElement
    {
        ISelectQuery SelectQuery { get; }

        ISelectClause Select { get; }

        IFromClause From { get; }


        IGroupByClause GroupBy { get; }

        IWhereClause Having { get; }

        IOrderByClause OrderBy { get; }

        ISelectQuery End();

        void SetSqlQuery(ISelectQuery selectQuery);
    }
}