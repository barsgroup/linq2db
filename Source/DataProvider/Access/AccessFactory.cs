using System.Collections.Specialized;

namespace LinqToDB.DataProvider.Access
{
    using LinqToDB.Properties;

    [UsedImplicitly]
    class AccessFactory : IDataProviderFactory
    {
        IDataProvider IDataProviderFactory.GetDataProvider(NameValueCollection attributes)
        {
            return new AccessDataProvider();
        }
    }
}
