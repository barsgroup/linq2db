namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Clauses;
    using LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces;

    public interface IClauseWithConditionBase : IClauseBase
    {
        IWhereClause Where { get; }
    }

    public interface IClauseBase: IQueryElement
    {
        ISelectQuery SelectQuery { get; }

        ISelectClause Select { get; }

        IFromClause From { get; }


        GroupByClause GroupBy { get; }

        IWhereClause Having { get; }

        IOrderByClause OrderBy { get; }

        ISelectQuery End();

        void SetSqlQuery(ISelectQuery selectQuery);
    }
}