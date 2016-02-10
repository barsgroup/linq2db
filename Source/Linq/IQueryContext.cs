using System.Collections.Generic;

namespace LinqToDB.Linq
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;

    public interface IQueryContext
	{
        ISelectQuery   SelectQuery { get; }
		object         Context     { get; set; }
		List<string>   QueryHints  { get; }
        ISqlParameter[] GetParameters();
	}
}
