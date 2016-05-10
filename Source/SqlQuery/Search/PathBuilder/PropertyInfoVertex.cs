using System.Collections.Generic;
using System.Reflection;

namespace Bars2Db.SqlQuery.Search.PathBuilder
{
    public class PropertyInfoVertex
    {
        public PropertyInfoVertex(PropertyInfo property)
        {
            Property = property;
        }

        public PropertyInfo Property { get; }

        public bool IsRoot { get; set; }
        public bool IsFinal { get; set; }

        public HashSet<PropertyInfoVertex> Parents { get; } = new HashSet<PropertyInfoVertex>();
        public HashSet<PropertyInfoVertex> Children { get; } = new HashSet<PropertyInfoVertex>();

        public override string ToString()
        {
            return $"{Property.DeclaringType.Name}.{Property.Name}";
        }
    }
}