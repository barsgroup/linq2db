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

        void SetSqlQuery(ISelectQuery selectQuery);
    }
}