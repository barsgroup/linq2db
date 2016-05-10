using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces
{
    public interface ISqlBinaryExpression : IQueryExpression
    {
        [SearchContainer]
        IQueryExpression Expr1 { get; set; }

        string Operation { get; }

        [SearchContainer]
        IQueryExpression Expr2 { get; set; }
    }
}