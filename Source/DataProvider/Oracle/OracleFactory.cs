using System.Collections.Specialized;
using Bars2Db.Properties;

namespace Bars2Db.DataProvider.Oracle
{
    [UsedImplicitly]
    internal class OracleFactory : IDataProviderFactory
    {
        IDataProvider IDataProviderFactory.GetDataProvider(NameValueCollection attributes)
        {
            for (var i = 0; i < attributes.Count; i++)
                if (attributes.GetKey(i) == "assemblyName")
                    OracleTools.AssemblyName = attributes.Get(i);

            return new OracleDataProvider();
        }
    }
}