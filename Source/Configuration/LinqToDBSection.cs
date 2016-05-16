using System.Configuration;
using System.Security;

namespace Bars2Db.Configuration
{
    /// <summary>
    ///     Implementation of custom configuration section.
    /// </summary>
    public class LinqToDBSection : ConfigurationSection
    {
        private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        private static readonly ConfigurationProperty _propDataProviders = new ConfigurationProperty("dataProviders",
            typeof(DataProviderElementCollection), new DataProviderElementCollection(),
            ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propDefaultConfiguration =
            new ConfigurationProperty("defaultConfiguration", typeof(string), null, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propDefaultDataProvider =
            new ConfigurationProperty("defaultDataProvider", typeof(string), null, ConfigurationPropertyOptions.None);

        private static LinqToDBSection _instance;

        static LinqToDBSection()
        {
            _properties.Add(_propDataProviders);
            _properties.Add(_propDefaultConfiguration);
            _properties.Add(_propDefaultDataProvider);
        }

        public static LinqToDBSection Instance
        {
            get
            {
                if (_instance == null)
                {
                    try
                    {
                        _instance = (LinqToDBSection) ConfigurationManager.GetSection("linq2db");
                    }
                    catch (SecurityException)
                    {
                        return null;
                    }
                }

                return _instance;
            }
        }

        protected override ConfigurationPropertyCollection Properties => _properties;

        public DataProviderElementCollection DataProviders => (DataProviderElementCollection) base[_propDataProviders];

        public string DefaultConfiguration => (string) base[_propDefaultConfiguration];

        public string DefaultDataProvider => (string) base[_propDefaultDataProvider];
    }
}