using System.Collections.Specialized;

namespace LinqToDB.DataProvider.SQLite
{
    using LinqToDB.Properties;

    [UsedImplicitly]
	class SQLiteFactory: IDataProviderFactory
	{
		IDataProvider IDataProviderFactory.GetDataProvider(NameValueCollection attributes)
		{
			return new SQLiteDataProvider();
		}
	}
}
