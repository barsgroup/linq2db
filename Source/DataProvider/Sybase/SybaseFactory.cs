using System.Collections.Specialized;

namespace LinqToDB.DataProvider.Sybase
{
    using LinqToDB.Properties;

    [UsedImplicitly]
	class SybaseFactory : IDataProviderFactory
	{
		IDataProvider IDataProviderFactory.GetDataProvider(NameValueCollection attributes)
		{
			for (var i = 0; i < attributes.Count; i++)
				if (attributes.GetKey(i) == "assemblyName")
					SybaseTools.AssemblyName = attributes.Get(i);

			return new SybaseDataProvider();
		}
	}
}
