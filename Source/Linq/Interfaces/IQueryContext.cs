using System.Collections.Generic;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.Linq.Interfaces
{
    public interface IQueryContext
    {
        ISelectQuery SelectQuery { get; }
        object Context { get; set; }
        List<string> QueryHints { get; }
        ISqlParameter[] GetParameters();
    }
}