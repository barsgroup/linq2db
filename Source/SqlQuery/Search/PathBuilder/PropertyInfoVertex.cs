namespace LinqToDB.SqlQuery.Search.PathBuilder
{
    using System.Collections.Generic;
    using System.Reflection;

    public class PropertyInfoVertex
    {
        public PropertyInfo Property { get; }

        public bool IsRoot { get; set; }
        public bool IsFinal { get; set; }

        public HashSet<PropertyInfoVertex> Parents { get; } = new HashSet<PropertyInfoVertex>();
        public HashSet<PropertyInfoVertex> Children { get; } = new HashSet<PropertyInfoVertex>();

        public PropertyInfoVertex(PropertyInfo property)
        {
            Property = property;
        }
        
        public override string ToString()
        {
            return $"{Property.DeclaringType.Name}.{Property.Name}";
        }
    }
}