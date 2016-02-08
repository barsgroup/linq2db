using System.Collections.Specialized;

namespace LinqToDB.DataProvider.MySql
{
    using LinqToDB.Properties;

    [UsedImplicitly]
	class MySqlFactory : IDataProviderFactory
	{
		IDataProvider IDataProviderFactory.GetDataProvider(NameValueCollection attributes)
		{
			return new MySqlDataProvider();
		}
	}
}
