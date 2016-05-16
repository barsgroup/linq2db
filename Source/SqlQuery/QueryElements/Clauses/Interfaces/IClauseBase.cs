using Bars2Db.SqlQuery.QueryElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Clauses.Interfaces
{
    public interface IClauseWithConditionBase : IClauseBase
    {
        IWhereClause Where { get; }
    }

    public interface IClauseBase : IQueryElement
    {
        ISelectQuery SelectQuery { get; }

        void SetSqlQuery(ISelectQuery selectQuery);
    }
}