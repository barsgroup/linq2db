using System.Collections.Generic;

namespace LinqToDB.ServiceModel
{
    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;

    public class LinqServiceQuery
	{
		public ISelectQuery  Query      { get; set; }
		public SqlParameter[] Parameters { get; set; }
		public List<string>   QueryHints { get; set; }
	}
}
