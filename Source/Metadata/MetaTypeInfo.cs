using System;
using System.Collections.Generic;
using System.Linq;

namespace Bars2Db.Metadata
{
    internal class MetaTypeInfo
    {
        public AttributeInfo[] Attributes;
        public Dictionary<string, MetaMemberInfo> Members;

        public string Name;

        public MetaTypeInfo(string name, Dictionary<string, MetaMemberInfo> members, params AttributeInfo[] attributes)
        {
            Name = name;
            Members = members;
            Attributes = attributes;
        }

        public AttributeInfo[] GetAttribute(Type type)
        {
            return
                Attributes.Where(a => a.Name == type.FullName).Concat(
                    Attributes.Where(a => a.Name == type.Name).Concat(
                        type.Name.EndsWith("Attribute")
                            ? Attributes.Where(
                                a => a.Name == type.Name.Substring(0, type.Name.Length - "Attribute".Length))
                            : Enumerable.Empty<AttributeInfo>())
                    ).ToArray();
        }
    }
}