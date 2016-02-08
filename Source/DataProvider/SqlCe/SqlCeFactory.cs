using System.Collections.Specialized;

namespace LinqToDB.DataProvider.SqlCe
{
    using LinqToDB.Properties;

    [UsedImplicitly]
	class SqlCeFactory : IDataProviderFactory
	{
		IDataProvider IDataProviderFactory.GetDataProvider(NameValueCollection attributes)
		{
			return new SqlCeDataProvider();
		}
	}
}
