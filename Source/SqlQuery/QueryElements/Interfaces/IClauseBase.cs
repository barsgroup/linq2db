namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Clauses;

    public interface IClauseBase: IQueryElement
    {
        ISelectQuery SelectQuery { get; }

        SelectClause Select { get; }

        IFromClause From { get; }

        WhereClause Where { get; }

        GroupByClause GroupBy { get; }

        WhereClause Having { get; }

        IOrderByClause OrderBy { get; }

        ISelectQuery End();

        void SetSqlQuery(ISelectQuery selectQuery);
    }
}