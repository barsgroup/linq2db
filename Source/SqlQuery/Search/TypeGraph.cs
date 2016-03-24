namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    using LinqToDB.Extensions;

    public class TypeGraph<TBaseSearchInterface>
    {
        public readonly Dictionary<Type, TypeVertex> SearchVertices = new Dictionary<Type, TypeVertex>();

        public readonly Dictionary<PropertyInfo, HashSet<Edge>> AllEdges = new Dictionary<PropertyInfo, HashSet<Edge>>();

        public readonly bool[][] TransitiveClosure;
        public readonly bool[][] ExtendedTransitiveClosure;

        public readonly TypeVertex[] Vertices;

        public int VertextCount => Vertices.Length;

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

        public bool PathExists(TypeVertex sourceVertex, TypeVertex searchVertex)
        {
            return TransitiveClosure[sourceVertex.Index][searchVertex.Index];
        }

        public bool ExtendedPathExists(TypeVertex sourceVertex, TypeVertex searchVertex)
        {
            return ExtendedTransitiveClosure[sourceVertex.Index][searchVertex.Index];
        }

        public bool IsFinalVertex(TypeVertex vertex, Type searchType)
        {
            return SearchHelper<TBaseSearchInterface>.FindHierarchy(searchType).Contains(vertex.Type);
        }

        public Dictionary<PropertyInfo, List<Edge>> GetEdgeSubTree(IEnumerable<Type> sourceTypes, Type searchType)
        {
            var sourceTypeVertices = sourceTypes.Select(t => SearchVertices[t]).ToList();
            var searchVertex = SearchVertices[searchType];

            var dict = new Dictionary<PropertyInfo, List<Edge>>();

            foreach (var propertyInfoGroup in AllEdges)
            {
                var edges =
                    propertyInfoGroup.Value.Where(
                        e =>
                        sourceTypeVertices.Any(v => PathExists(v, e.Parent)) && ExtendedPathExists(e.Parent, searchVertex)
                        && (ExtendedPathExists(e.Child, searchVertex) || IsFinalVertex(e.Child, searchType))).ToList();

                if (edges.Count > 0)
                {
                    dict[propertyInfoGroup.Key] = edges;
                }
            }

            return dict;
        }

        public TypeGraph(IEnumerable<Type> types)
        {
            var inter = types.Where(t => typeof(TBaseSearchInterface).IsAssignableFrom(t) && t.IsInterface).SelectMany(SearchHelper<TBaseSearchInterface>.FindInterfacesWithSelf).Distinct().ToList();
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

                    HashSet<Edge> edgeSet;
                    if (!AllEdges.TryGetValue(info, out edgeSet))
                    {
                        edgeSet = new HashSet<Edge>();
                        AllEdges[info] = edgeSet;
                    }

                    var childCastInterfaces = SearchHelper<TBaseSearchInterface>.FindInterfacesWithSelf(propertyType);

                    foreach (var childCastInterface in childCastInterfaces)
                    {
                        TypeVertex childVertex;
                        if (!SearchVertices.TryGetValue(childCastInterface, out childVertex))
                        {
                            childVertex = SearchVertices[childCastInterface] = new TypeVertex(childCastInterface, counter++);
                            Vertices[childVertex.Index] = childVertex;
                        }
                        var edge = new Edge(vertex, info, childVertex);
                        vertex.Children.AddLast(edge);
                        childVertex.Parents.AddLast(edge);
                        edgeSet.Add(edge);
                    }
                }
            }

            if (counter != interfaces.Count)
            {
                throw new Exception("Количество интерфейсов не соответствует графу");
            }

            TransitiveClosure = BuildAdjacencyMatrix();
            FillTransitiveClosure(TransitiveClosure);

            ExtendedTransitiveClosure = BuildAdjacencyMatrixExtended();
            FillTransitiveClosure(ExtendedTransitiveClosure);
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
                vertex.Children.ForEach(
                    egde =>
                    {
                        matrix[vertex.Index][egde.Value.Child.Index] = true;
                    });
            }

            return matrix;
        }

        private bool[][] BuildAdjacencyMatrixExtended()
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
                vertex.Children.ForEach(
                    egde =>
                        {
                            var child = egde.Value.Child;

                            matrix[vertex.Index][child.Index] = true;
                            foreach (var type in SearchHelper<TBaseSearchInterface>.FindDerived(child.Type))
                            {
                                matrix[vertex.Index][SearchVertices[type].Index] = true;
                            }
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

        public LinkedList<Edge> Children { get; } = new LinkedList<Edge>(); // parent == this

        public LinkedList<Edge> Parents { get; } = new LinkedList<Edge>(); // child == this

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
    
    public class Edge : IEquatable<Edge>
    {
        public TypeVertex Parent { get; }

        public TypeVertex Child { get; }

        public PropertyInfo PropertyInfo { get; }

        public Edge(TypeVertex parent, PropertyInfo property, TypeVertex child)
        {
            Parent = parent;
            PropertyInfo = property;
            Child = child;
        }
        
        public bool Equals(Edge other)
        {
            return Parent.Type == other.Parent.Type && PropertyInfo == other.PropertyInfo && Child.Type == other.Child.Type;
        }
        
        public override bool Equals(object obj)
        {
            var edge = obj as Edge;

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