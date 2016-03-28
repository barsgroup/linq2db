namespace LinqToDB.SqlQuery.Search.SearchEx
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using LinqToDB.Extensions;

    public class TypeGraphEx<TBaseSearchInterface>
    {
        public readonly Dictionary<Type, TypeVertex> SearchVertices = new Dictionary<Type, TypeVertex>();
        
        public readonly HashSet<PropertyInfo> AllPropertyInfos = new HashSet<PropertyInfo>();

        public readonly bool[][] TransitiveClosure;

        public readonly TypeVertex[] Vertices;

        public int VertexCount => Vertices.Length;

        public TypeVertex GetTypeVertex(Type type)
        {
            if (!SearchVertices.ContainsKey(type))
            {
                throw new ArgumentException("Type is not in graph");
            }

            return SearchVertices[type];
        }

        public TypeVertex GetTypeVertex(int index)
        {
            return Vertices[index];
        }

        public TypeGraphEx(IEnumerable<Type> types)
        {
            var inter = types.Where(t => typeof(TBaseSearchInterface).IsAssignableFrom(t) && t.IsInterface).SelectMany(SearchHelper<TBaseSearchInterface>.FindBaseWithSelf).Distinct().ToList();
            var interfaces =
                inter.SelectMany(t => t.GetProperties())
                     .Where(p => p.GetCustomAttribute<SearchContainerAttribute>() != null)
                     .Select(p => p.DeclaringType)
                     .Concat(inter)
                     .Distinct()
                     .ToList();

            Vertices = new TypeVertex[interfaces.Count];

            var counter = 0;

            foreach (var intType in interfaces)
            {
                TypeVertex vertex;
                if (!SearchVertices.TryGetValue(intType, out vertex))
                {
                    vertex = SearchVertices[intType] = new TypeVertex(intType, counter++);
                    Vertices[vertex.Index] = vertex;
                }

                var castTypes = SearchHelper<TBaseSearchInterface>.FindBase(intType).Concat(SearchHelper<TBaseSearchInterface>.FindDerived(intType));

                foreach (var castType in castTypes)
                {
                    TypeVertex childVertex;
                    if (!SearchVertices.TryGetValue(castType, out childVertex))
                    {
                        childVertex = SearchVertices[castType] = new TypeVertex(castType, counter++);
                        Vertices[childVertex.Index] = childVertex;
                    }

                    vertex.Casts.AddLast(new CastEdge(vertex, childVertex));
                }

                var propertyInfos = intType.GetProperties().Where(p => p.GetCustomAttribute<SearchContainerAttribute>() != null);
                foreach (var info in propertyInfos)
                {
                    var propertyType = GetElementType(info.PropertyType);

                    if (!typeof(TBaseSearchInterface).IsAssignableFrom(propertyType) && !IsCollectionType(info.PropertyType))
                    {
                        throw new InvalidCastException();
                    }

                    if (!propertyType.IsInterface)
                    {
                        throw new InvalidOperationException("Все свойства интерфейсы");
                    }

                    AllPropertyInfos.Add(info);

                    TypeVertex childVertex;
                    if (!SearchVertices.TryGetValue(propertyType, out childVertex))
                    {
                        childVertex = SearchVertices[propertyType] = new TypeVertex(propertyType, counter++);
                        Vertices[childVertex.Index] = childVertex;
                    }

                    var edge = new PropertyEdge(vertex, info, childVertex);
                    vertex.Children.AddLast(edge);
                    childVertex.Parents.AddLast(edge);
                }
            }

            if (counter != interfaces.Count)
            {
                throw new Exception("Количество интерфейсов не соответствует графу");
            }

            TransitiveClosure = BuildAdjacencyMatrix();
            FillTransitiveClosure(TransitiveClosure);
        }

        public bool PathExists(TypeVertex from, TypeVertex to)
        {
            return TransitiveClosure[from.Index][to.Index];
        }

        public void FillTransitiveClosure(bool[][] adjacencyMatrix)
        {
            var interfacesCount = Vertices.Length;

            for (var k = 0; k < interfacesCount; k++)
            {
                for (var i = 0; i < interfacesCount; i++)
                {
                    for (var j = 0; j < interfacesCount; j++)
                    {
                        adjacencyMatrix[i][j] = adjacencyMatrix[i][j] || adjacencyMatrix[i][k] && adjacencyMatrix[k][j];
                    }
                }
            }
        }

        private bool[][] BuildAdjacencyMatrix()
        {
            var interfacesCount = Vertices.Length;

            var matrix = new bool[interfacesCount][];
            for (var i = 0; i < interfacesCount; i++)
            {
                matrix[i] = new bool[interfacesCount];
            }

            for (var i = 0; i < interfacesCount; i++)
            {
                var vertex = Vertices[i];
                matrix[i][i] = true;
                vertex.Children.ForEach(
                    egde =>
                    {
                        matrix[vertex.Index][egde.Value.Child.Index] = true;
                    });
                vertex.Casts.ForEach(
                    egde =>
                    {
                        matrix[vertex.Index][egde.Value.CastTo.Index] = true;
                    });
            }

            return matrix;
        }

        private static Type GetElementType(Type sourceType)
        {
            if (sourceType.IsArray)
            {
                return sourceType.GetElementType();
            }

            if (!sourceType.IsGenericType)
            {
                return sourceType;
            }

            if (typeof(LinkedList<>).IsAssignableFrom(sourceType.GetGenericTypeDefinition()))
            {
                return sourceType.GetGenericArguments()[0];
            }

            if (typeof(Dictionary<,>).IsAssignableFrom(sourceType.GetGenericTypeDefinition()))
            {
                return sourceType.GetGenericArguments()[1];
            }

            if (typeof(List<>).IsAssignableFrom(sourceType.GetGenericTypeDefinition()))
            {
                return sourceType.GetGenericArguments()[0];
            }

            return sourceType;
        }

        private static bool IsCollectionType(Type sourceType)
        {
            return GetElementType(sourceType) != sourceType;
        }
    }
    
    public class TypeVertex
    {
        public Type Type { get; }

        public int Index { get; }

        public LinkedList<PropertyEdge> Children { get; } = new LinkedList<PropertyEdge>(); // parent == this

        public LinkedList<PropertyEdge> Parents { get; } = new LinkedList<PropertyEdge>(); // child == this

        public LinkedList<CastEdge> Casts { get; } = new LinkedList<CastEdge>();

        public TypeVertex(Type type, int index)
        {
            Type = type;
            Index = index;
        }

        public override string ToString()
        {
            return $"({Index}) {Type.Name}";
        }
    }

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

    public class CastEdge : IEquatable<CastEdge>
    {
        public TypeVertex CastFrom { get; }

        public TypeVertex CastTo { get; }

        public CastEdge(TypeVertex castFrom, TypeVertex castTo)
        {
            CastFrom = castFrom;
            CastTo = castTo;
        }

        public virtual bool Equals(CastEdge other)
        {
            return CastFrom.Type == other.CastFrom.Type && CastTo.Type == other.CastTo.Type;
        }

        public override bool Equals(object obj)
        {
            var edge = obj as CastEdge;

            return edge != null && Equals(edge);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = CastFrom?.Type.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (CastTo?.Type.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{CastFrom.Type.Name} ~> {CastTo.Type.Name}";
        }
    }
}