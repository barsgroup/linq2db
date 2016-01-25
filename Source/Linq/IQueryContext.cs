using System.Collections.Generic;

namespace LinqToDB.Linq
{
    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.SqlElements;

    public interface IQueryContext
	{
		SelectQuery    SelectQuery { get; }
		object         Context     { get; set; }
		List<string>   QueryHints  { get; }
		SqlParameter[] GetParameters();
	}
}
