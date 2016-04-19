namespace LinqToDB.SqlQuery.Search.TypeGraph
{
    using System;
    using System.Reflection;

    public class PropertyEdge : IEquatable<PropertyEdge>
    {
        public TypeVertex Parent { get; }

        public TypeVertex Child { get; }

        public PropertyInfo PropertyInfo { get; }

        public PropertyEdge(TypeVertex parent, PropertyInfo property, TypeVertex child)
        {
            Parent = parent;
            PropertyInfo = property;
            Child = child;
        }

        public virtual bool Equals(PropertyEdge other)
        {
            return Parent.Type == other.Parent.Type && PropertyInfo == other.PropertyInfo && Child.Type == other.Child.Type;
        }

        public override bool Equals(object obj)
        {
            var edge = obj as PropertyEdge;

            return edge != null && Equals(edge);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Parent?.Type.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Child?.Type.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (PropertyInfo?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{Parent.Type.Name}.{PropertyInfo.Name} -> {Child.Type.Name} ({PropertyInfo.PropertyType.Name})";
        }
    }
}