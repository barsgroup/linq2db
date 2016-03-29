namespace LinqToDB.SqlQuery.Search.TypeGraph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search.Utils;

    public class TypeGraph<TBaseSearchInterface>
    {
        private readonly Dictionary<Type, TypeVertex> _searchVertices = new Dictionary<Type, TypeVertex>();

        private readonly HashSet<PropertyInfo> _allPropertyInfos = new HashSet<PropertyInfo>();

        private readonly Connection[][] _transitiveClosure;

        public readonly TypeVertex[] Vertices;

        private int VertexCount => Vertices.Length;

        public TypeVertex GetTypeVertex(Type type)
        {
            if (!_searchVertices.ContainsKey(type))
            {
                throw new ArgumentException("Type is not in graph");
            }

            return _searchVertices[type];
        }

        public TypeGraph(IEnumerable<Type> types)
        {
            var interfaces = SearchHelper<TBaseSearchInterface>.GetAllSearchInterfaces(types).ToList();

            Vertices = new TypeVertex[interfaces.Count];

            var counter = 0;

            foreach (var intType in interfaces)
            {
                TypeVertex vertex;
                if (!_searchVertices.TryGetValue(intType, out vertex))
                {
                    vertex = _searchVertices[intType] = new TypeVertex(intType, counter++);
                    Vertices[vertex.Index] = vertex;
                }

                var castTypes = SearchHelper<TBaseSearchInterface>.FindBase(intType).Concat(SearchHelper<TBaseSearchInterface>.FindDerived(intType));

                foreach (var castType in castTypes)
                {
                    TypeVertex childVertex;
                    if (!_searchVertices.TryGetValue(castType, out childVertex))
                    {
                        childVertex = _searchVertices[castType] = new TypeVertex(castType, counter++);
                        Vertices[childVertex.Index] = childVertex;
                    }

                    vertex.Casts.AddLast(new CastEdge(vertex, childVertex));
                }

                var propertyInfos = intType.GetProperties().Where(p => p.GetCustomAttribute<SearchContainerAttribute>() != null);
                foreach (var info in propertyInfos)
                {
                    var propertyType = CollectionUtils.GetElementType(info.PropertyType);

                    if (!typeof(TBaseSearchInterface).IsAssignableFrom(propertyType))
                    {
                        throw new InvalidCastException("Все должно наследоваться от базового интерфейса");
                    }

                    if (!propertyType.IsInterface)
                    {
                        throw new InvalidOperationException("Все свойства интерфейсы");
                    }

                    if (propertyType.IsGenericType)
                    {
                        throw new NotSupportedException("Generics are not supported");
                    }

                    _allPropertyInfos.Add(info);

                    TypeVertex childVertex;
                    if (!_searchVertices.TryGetValue(propertyType, out childVertex))
                    {
                        childVertex = _searchVertices[propertyType] = new TypeVertex(propertyType, counter++);
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

            _transitiveClosure = BuildAdjacencyMatrix();
            FillTransitiveClosure(_transitiveClosure);
        }

        public bool PathExists(TypeVertex from, TypeVertex to)
        {
            return !_transitiveClosure[from.Index][to.Index].IsEmpty();
        }

        private void FillTransitiveClosure(Connection[][] matrix)
        {
            var interfacesCount = VertexCount;

            for (var k = 0; k < interfacesCount; k++)
            {
                for (var i = 0; i < interfacesCount; i++)
                {
                    for (var j = 0; j < interfacesCount; j++)
                    {
                        var ik = matrix[i][k];
                        var kj = matrix[k][j];

                        if (ik.IsEmpty() || kj.IsEmpty())
                        {
                            continue;
                        }

                        if (ik.EndsWith(ConnectionType.Property) || kj.StartsWith(ConnectionType.Property)) // Запрещаем 2 последовательных каста
                        {
                            matrix[i][j] = matrix[i][j].Union(ik.StartType, kj.EndType);
                        }
                    }
                }
            }
        }

        private Connection[][] BuildAdjacencyMatrix()
        {
            var interfacesCount = VertexCount;

            var matrix = new Connection[interfacesCount][];
            for (var i = 0; i < interfacesCount; i++)
            {
                matrix[i] = new Connection[interfacesCount];
                matrix[i][i] = new Connection(ConnectionType.Cast, ConnectionType.Cast);
            }

            for (var i = 0; i < interfacesCount; i++)
            {
                var vertex = Vertices[i];

                vertex.Casts.ForEach(egde =>
                    {
                        matrix[vertex.Index][egde.Value.CastTo.Index] = matrix[vertex.Index][egde.Value.CastTo.Index].Union(ConnectionType.Cast, ConnectionType.Cast);
                    });

                vertex.Children.ForEach(egde =>
                    {
                        matrix[vertex.Index][egde.Value.Child.Index] = matrix[vertex.Index][egde.Value.Child.Index].Union(ConnectionType.Property, ConnectionType.Property);
                    });
            }

            return matrix;
        }

       
    }
}