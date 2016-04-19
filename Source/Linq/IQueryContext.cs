using System.Collections.Generic;

namespace LinqToDB.Linq
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IQueryContext
    {
        ISelectQuery   SelectQuery { get; }
        object         Context     { get; set; }
        List<string>   QueryHints  { get; }
        ISqlParameter[] GetParameters();
    }
}
