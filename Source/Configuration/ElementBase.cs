using System;
using System.Collections.Specialized;
using System.Configuration;

namespace Bars2Db.Configuration
{
    public abstract class ElementBase : ConfigurationElement
    {
        private readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        protected override ConfigurationPropertyCollection Properties => _properties;
        public NameValueCollection Attributes { get; } = new NameValueCollection(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     Gets a value indicating whether an unknown attribute is encountered during deserialization.
        /// </summary>
        /// <returns>
        ///     True when an unknown attribute is encountered while deserializing.
        /// </returns>
        /// <param name="name">The name of the unrecognized attribute.</param>
        /// <param name="value">The value of the unrecognized attribute.</param>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            var property = new ConfigurationProperty(name, typeof(string), value);

            _properties.Add(property);

            base[property] = value;

            Attributes.Add(name, value);

            return true;
        }
    }
}