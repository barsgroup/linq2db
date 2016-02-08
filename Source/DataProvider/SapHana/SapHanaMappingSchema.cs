namespace LinqToDB.DataProvider.SapHana
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements;

    using Mapping;

    public class SapHanaMappingSchema : MappingSchema
	{
		public SapHanaMappingSchema() : this(ProviderName.SapHana)
		{
		}

		protected SapHanaMappingSchema(string configuration) : base(configuration)
		{
			SetDataType(typeof(string), new SqlDataType(DataType.NVarChar, typeof(string), 255));
		}
	}
}
