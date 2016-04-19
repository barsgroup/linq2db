using System.Collections.Specialized;

namespace LinqToDB.DataProvider.Informix
{
    using LinqToDB.Properties;

    [UsedImplicitly]
	class InformixFactory : IDataProviderFactory
	{
		IDataProvider IDataProviderFactory.GetDataProvider(NameValueCollection attributes)
		{
			return new InformixDataProvider();
		}
	}
}
