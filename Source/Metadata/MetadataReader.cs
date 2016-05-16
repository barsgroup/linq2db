using System;
using System.Linq;
using System.Reflection;
using Bars2Db.Properties;

namespace Bars2Db.Metadata
{
    public class MetadataReader : IMetadataReader
    {
        public static MetadataReader Default = new MetadataReader(
            new AttributeReader()
#if !SILVERLIGHT && !NETFX_CORE
            , new SystemDataLinqAttributeReader()
#endif
            );

        private readonly IMetadataReader[] _readers;

        public MetadataReader([NotNull] params IMetadataReader[] readers)
        {
            if (readers == null) throw new ArgumentNullException(nameof(readers));

            _readers = readers;
        }

        public T[] GetAttributes<T>(Type type, bool inherit)
            where T : Attribute
        {
            return _readers.SelectMany(r => r.GetAttributes<T>(type, inherit)).ToArray();
        }

        public T[] GetAttributes<T>(MemberInfo memberInfo, bool inherit)
            where T : Attribute
        {
            return _readers.SelectMany(r => r.GetAttributes<T>(memberInfo, inherit)).ToArray();
        }
    }
}