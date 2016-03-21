namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using LinqToDB.Extensions;

    internal class TypeGraph<TBaseSearchInterface>
    {
        internal readonly Dictionary<Type, TypeVertex> SearchVertices = new Dictionary<Type, TypeVertex>();

        internal bool[][] TransitiveClosure;

        internal TypeVertex[] Vertices;

        public bool PathExists(TypeVertex sourceVertex, TypeVertex searchVertex)
        {
            return TransitiveClosure[sourceVertex.Index][searchVertex.Index];
        }

        public TypeGraph(IEnumerable<Type> types)
        {
            var inter = types.Where(t => typeof(TBaseSearchInterface).IsAssignableFrom(t) && t.IsInterface).SelectMany(FindInterfacesWithSelf).ToList();
            var interfaces =
                inter.SelectMany(t => t.GetProperties())
                     .Where(p => p.GetCustomAttribute<SearchContainerAttribute>() != null)
                     .Select(p => p.DeclaringType)
                     .Concat(inter)
                     .Distinct()
                     .ToList();

            Vertices = new TypeVertex[interfaces.Count];

            foreach (var intType in interfaces)
            {
                TypeVertex vertex;
                if (!SearchVertices.TryGetValue(intType, out vertex))
                {
                    vertex = SearchVertices[intType] = new TypeVertex(intType);
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

                    var castInterfaces = FindInterfacesWithSelf(propertyType);

                    foreach (var castInterface in castInterfaces)
                    {
                        TypeVertex childVertex;
                        if (!SearchVertices.TryGetValue(castInterface, out childVertex))
                        {
                            childVertex = SearchVertices[castInterface] = new TypeVertex(castInterface);
                            Vertices[childVertex.Index] = childVertex;
                        }
                        vertex.Children.AddLast(Tuple.Create(info, childVertex));
                    }
                }
            }

            if (TypeVertex.Counter != interfaces.Count)
            {
                throw new Exception("Количество интерфейсов не соответствует графу");
            }

            TransitiveClosure = BuildTransitiveClosure();
        }

        private bool[][] BuildTransitiveClosure()
        {
            var interfacesCount = Vertices.Length;

            var matrix = new bool[interfacesCount][];
            for (var i = 0; i < interfacesCount; i++)
            {
                matrix[i] = new bool[interfacesCount];
            }
            FillAdjacencyMatrix(matrix);

            for (var k = 0; k < interfacesCount; k++)
            {
                for (var i = 0; i < interfacesCount; i++)
                {
                    for (var j = 0; j < interfacesCount; j++)
                    {
                        matrix[i][j] = matrix[i][j] || matrix[i][k] && matrix[k][j];
                    }
                }
            }

            return matrix;
        }

        private void FillAdjacencyMatrix(bool[][] matrix)
        {
            for (var i = 0; i < matrix.Length; i++)
            {
                var vertex = Vertices[i];
                vertex.Children.ForEach(
                    child =>
                    {
                        var node = child.Value.Item2;
                        var interfaces = FindInterfacesWithSelf(node.Type);
                        foreach (var inter in interfaces)
                        {
                            matrix[vertex.Index][SearchVertices[inter].Index] = true;
                        }
                    });
            }
        }

        private static IEnumerable<Type> FindInterfaces(Type propertyType)
        {
            return propertyType.GetInterfaces().Where(typeof(TBaseSearchInterface).IsAssignableFrom);
        }

        private static IEnumerable<Type> FindInterfacesWithSelf(Type propertyType)
        {
            var interfaces = FindInterfaces(propertyType);

            return propertyType.IsInterface
                       ? interfaces.Concat(new[] { propertyType })
                       : interfaces;
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

    internal class TypeVertex
    {
        public static int Counter { get; private set; }

        public Type Type { get; }

        public int Index { get; }

        public LinkedList<Tuple<PropertyInfo, TypeVertex>> Children { get; } = new LinkedList<Tuple<PropertyInfo, TypeVertex>>();

        public TypeVertex(Type type)
        {
            Type = type;
            Index = Counter++;
        }
    }
}