using System.Collections.Generic;

namespace LinqToDB.ServiceModel
{
    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.SqlElements;

    public class LinqServiceQuery
	{
		public SelectQuery    Query      { get; set; }
		public SqlParameter[] Parameters { get; set; }
		public List<string>   QueryHints { get; set; }
	}
}
