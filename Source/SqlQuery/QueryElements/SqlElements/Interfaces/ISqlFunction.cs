using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces
{
    public interface ISqlFunction : IQueryExpression
    {
        string Name { get; }

        [SearchContainer]
        IQueryExpression[] Parameters { get; }
    }
}