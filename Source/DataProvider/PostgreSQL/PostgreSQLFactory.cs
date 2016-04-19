using System.Collections.Specialized;

namespace LinqToDB.DataProvider.PostgreSQL
{
    using LinqToDB.Properties;

    [UsedImplicitly]
    class PostgreSQLFactory: IDataProviderFactory
    {
        IDataProvider IDataProviderFactory.GetDataProvider(NameValueCollection attributes)
        {
            return new PostgreSQLDataProvider();
        }
    }
}
