namespace LinqToDB.Linq
{
	using Data;
	using DataProvider;
	using SqlProvider;
	using Mapping;

	class DefaultDataContextInfo : IDataContextInfo
	{
		private IDataContext    _dataContext;
		public  IDataContext     DataContext => _dataContext ?? (_dataContext = new DataConnection());

	    public MappingSchema     MappingSchema => MappingSchema.Default;

	    public bool              DisposeContext => true;

	    public SqlProviderFlags  SqlProviderFlags => _dataProvider.SqlProviderFlags;

	    public string            ContextID => _dataProvider.Name;

	    public ISqlBuilder CreateSqlBuilder()
		{
			return _dataProvider.CreateSqlBuilder();
		}

		public ISqlOptimizer GetSqlOptimizer()
		{
			return _dataProvider.GetSqlOptimizer();
		}

		public IDataContextInfo Clone(bool forNestedQuery)
		{
			return new DataContextInfo(DataContext.Clone(forNestedQuery));
		}

		static readonly IDataProvider _dataProvider = DataConnection.GetDataProvider(DataConnection.DefaultConfiguration);
	}
}
