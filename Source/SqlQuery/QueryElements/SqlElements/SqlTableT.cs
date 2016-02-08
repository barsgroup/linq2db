namespace LinqToDB.SqlQuery.QueryElements.SqlElements
{
    using LinqToDB.Mapping;

    public class SqlTable<T> : SqlTable
	{
		public SqlTable()
			: base(typeof(T))
		{
		}

		public SqlTable(MappingSchema mappingSchema)
			: base(mappingSchema, typeof(T))
		{
		}
	}
}
