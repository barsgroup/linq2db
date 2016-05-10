using System.Collections.Specialized;

namespace Bars2Db.DataProvider
{
    public interface IDataProviderFactory
    {
        IDataProvider GetDataProvider(NameValueCollection attributes);
    }
}