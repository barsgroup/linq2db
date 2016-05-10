using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces
{
    public interface ISqlExpression : IQueryExpression
    {
        string Expr { get; }

        [SearchContainer]
        IQueryExpression[] Parameters { get; }
    }
}