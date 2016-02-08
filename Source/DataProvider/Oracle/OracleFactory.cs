using System.Collections.Specialized;

namespace LinqToDB.DataProvider.Oracle
{
    using LinqToDB.Properties;

    [UsedImplicitly]
	class OracleFactory : IDataProviderFactory
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
