using System.Collections.Generic;

namespace LinqToDB.ServiceModel
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class LinqServiceQuery
	{
		public ISelectQuery  Query      { get; set; }
		public ISqlParameter[] Parameters { get; set; }
		public List<string>   QueryHints { get; set; }
	}
}
