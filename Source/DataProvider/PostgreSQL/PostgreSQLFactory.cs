using System.Collections.Specialized;
using Bars2Db.Properties;

namespace Bars2Db.DataProvider.PostgreSQL
{
    [UsedImplicitly]
    internal class PostgreSQLFactory : IDataProviderFactory
    {
        IDataProvider IDataProviderFactory.GetDataProvider(NameValueCollection attributes)
        {
            return new PostgreSQLDataProvider();
        }
    }
}