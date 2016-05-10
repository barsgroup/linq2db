using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bars2Db.Common;
using Bars2Db.Mapping;

namespace Bars2Db.Metadata
{
    public class FluentMetadataReader : IMetadataReader
    {
        private readonly ConcurrentDictionary<MemberInfo, List<Attribute>> _members =
            new ConcurrentDictionary<MemberInfo, List<Attribute>>();

        private readonly ConcurrentDictionary<Type, List<Attribute>> _types =
            new ConcurrentDictionary<Type, List<Attribute>>();

        public T[] GetAttributes<T>(Type type, bool inherit = true)
            where T : Attribute
        {
            List<Attribute> attrs;
            return _types.TryGetValue(type, out attrs) ? attrs.OfType<T>().ToArray() : Array<T>.Empty;
        }

        public T[] GetAttributes<T>(MemberInfo memberInfo, bool inherit = true)
            where T : Attribute
        {
            List<Attribute> attrs;
            return _members.TryGetValue(memberInfo, out attrs) ? attrs.OfType<T>().ToArray() : Array<T>.Empty;
        }

        public void AddAttribute(Type type, Attribute attribute)
        {
            _types.GetOrAdd(type, t => new List<Attribute>()).Add(attribute);
        }

        public void AddAttribute(MemberInfo memberInfo, Attribute attribute)
        {
            var prop = memberInfo as PropertyInfo;
            var mappingSchema = MappingSchema.Default;
            if (prop != null && mappingSchema.IsScalarType(prop.PropertyType) &&
                (attribute is AssociationAttribute ||
                 (attribute is ColumnAttribute && ((ColumnAttribute) attribute).Transparent)))
            {
                return;
            }

            _members.GetOrAdd(memberInfo, t => new List<Attribute>()).Add(attribute);
        }
    }
}