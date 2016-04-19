using System.Collections.Specialized;

namespace LinqToDB.DataProvider.Firebird
{
    using LinqToDB.Properties;

    [UsedImplicitly]
	class FirebirdFactory: IDataProviderFactory
	{
		IDataProvider IDataProviderFactory.GetDataProvider(NameValueCollection attributes)
		{
			return new FirebirdDataProvider();
		}
	}
}
