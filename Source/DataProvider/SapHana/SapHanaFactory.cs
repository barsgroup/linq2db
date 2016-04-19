using System.Collections.Specialized;

namespace LinqToDB.DataProvider.SapHana
{
    using LinqToDB.Properties;

    [UsedImplicitly]
	class SapHanaFactory : IDataProviderFactory
	{
		IDataProvider IDataProviderFactory.GetDataProvider(NameValueCollection attributes)
		{
			for (var i = 0; i < attributes.Count; i++)
				if (attributes.GetKey(i) == "assemblyName")
					SapHanaTools.AssemblyName = attributes.Get(i);

			return new SapHanaDataProvider();
		}
	}
}
