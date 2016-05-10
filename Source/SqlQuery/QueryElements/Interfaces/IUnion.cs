using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Interfaces
{
    public interface IUnion : IQueryElement
    {
        [SearchContainer]
        ISelectQuery SelectQuery { get; }

        bool IsAll { get; }
    }
}