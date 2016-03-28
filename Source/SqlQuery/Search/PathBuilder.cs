namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using LinqToDB.Extensions;

    public class PathBuilderSearchCache
    {
        public Type SourceType { get; }

        public Type SearchType { get; }

        public Dictionary<TypeVertex, HashSet<PropertyInfoVertex>> AllVertices { get; } = new Dictionary<TypeVertex, HashSet<PropertyInfoVertex>>();

        public Dictionary<PropertyInfo, PropertyInfoVertex> AllProperties { get; } = new Dictionary<PropertyInfo, PropertyInfoVertex>();
        public Dictionary<PropertyInfo, CompositPropertyVertex> AllCompositProperties { get; } = new Dictionary<PropertyInfo, CompositPropertyVertex>();
        
        public LinkedList<CompositPropertyVertex> OptimizedPaths { get; set; }
        
        public PathBuilderSearchCache(Type sourceType, Type searchType)
        {
            SourceType = sourceType;
            SearchType = searchType;
        }
    }

    public class PathBuilder<TBaseSearchInterface>
    {
        private readonly TypeGraph<TBaseSearchInterface> _typeGraph;

        private readonly Dictionary<Tuple<Type, Type>, PathBuilderSearchCache> _cache = new Dictionary<Tuple<Type, Type>, PathBuilderSearchCache>(); 

        public PathBuilder(TypeGraph<TBaseSearchInterface> typeGraph)
        {
            _typeGraph = typeGraph;
        }

        public LinkedList<CompositPropertyVertex> Find(TBaseSearchInterface source, Type searchType)
        {
            var sourceType = source.GetType();
            var key = Tuple.Create(sourceType, searchType);

            PathBuilderSearchCache cachedResult;
            if (_cache.TryGetValue(key, out cachedResult))
            {
                return cachedResult.OptimizedPaths;
            }

            cachedResult = new PathBuilderSearchCache(sourceType, searchType);
            _cache[key] = cachedResult;

            var sourceTypes = SearchHelper<TBaseSearchInterface>.FindLeafInterfaces(sourceType);

            var paths = GetOrBuildPaths(sourceTypes, searchType, cachedResult);
            var optimized = OptimizePaths(paths, cachedResult);

            cachedResult.OptimizedPaths = optimized;

            return optimized;
        }

        public HashSet<PropertyInfoVertex> GetOrBuildPaths(IEnumerable<Type> sourceTypes, Type searchType, PathBuilderSearchCache cache)
        {
            var propertyPathsSet = new HashSet<PropertyInfoVertex>();
            var searchVertex = _typeGraph.GetTypeVertex(searchType);
            
            foreach (var sourceType in sourceTypes)
            {
                var sourceVertex = _typeGraph.GetTypeVertex(sourceType);

                if (cache.AllVertices.ContainsKey(sourceVertex))
                {
                    propertyPathsSet.UnionWith(cache.AllVertices[sourceVertex]);
                    continue;
                }

                if (!_typeGraph.PathExists(sourceVertex, searchVertex))
                {
                    continue;
                }

                var properties = BuildSearchTree(sourceVertex, searchVertex, cache);

                if (properties.Count == 0)
                {
                    throw new InvalidOperationException("Не найден ни один путь");
                }
                
                propertyPathsSet.UnionWith(properties);
            }

            if (propertyPathsSet.Count == 0)
            {
                throw new InvalidOperationException("Не найден ни один путь");
            }
            
            foreach (var node in propertyPathsSet)
            {
                node.IsRoot = true;
            }

            var hierarchyTypes = SearchHelper<TBaseSearchInterface>.FindHierarchy(searchType);
            foreach (var type in hierarchyTypes)
            {
                var typeVertex = _typeGraph.GetTypeVertex(type);
                HashSet<PropertyInfoVertex> edges;

                if (!cache.AllVertices.TryGetValue(typeVertex, out edges))
                {
                    continue;
                }

                foreach (var parent in edges.SelectMany(edge => edge.Parents))
                {
                    parent.IsFinal = true;
                }
            }

            return propertyPathsSet;
        }

        public static LinkedList<CompositPropertyVertex> OptimizePaths(HashSet<PropertyInfoVertex> propertyPaths, PathBuilderSearchCache cache)
        {
            var optimizePaths = new LinkedList<CompositPropertyVertex>();

            foreach (var node in propertyPaths)
            {
                optimizePaths.AddLast(OptimizeNode(node, cache));
            }

            return optimizePaths;
        }
        
        private static CompositPropertyVertex OptimizeNode(PropertyInfoVertex node, PathBuilderSearchCache cache)
        {
            CompositPropertyVertex composite;
            if (cache.AllCompositProperties.TryGetValue(node.Property, out composite))
            {
                return composite;
            }

            composite = new CompositPropertyVertex();
            cache.AllCompositProperties[node.Property] = composite;

            composite.PropertyList.AddLast(node.Property);

            var current = node;

            while (current.Children.Count == 1 && !current.IsFinal)
            {
                var next = current.Children.First();

                if (next.IsRoot || next.Parents.Count > 1 || cache.AllCompositProperties.ContainsKey(next.Property))
                {
                    break;
                }

                current = next;

                composite.PropertyList.AddLast(current.Property);
            }

            foreach (var listNode in current.Children)
            {
                composite.Children.AddLast(OptimizeNode(listNode, cache));
            }
            
            return composite;
        }

        public HashSet<PropertyInfoVertex> BuildSearchTree(
            TypeVertex currentVertex,
            TypeVertex searchVertex,
            PathBuilderSearchCache cache)
        {
            var properties = new HashSet<PropertyInfoVertex>();

            HashSet<PropertyInfoVertex> cachedProperties;
            if (cache.AllVertices.TryGetValue(currentVertex, out cachedProperties))
            {
                return cachedProperties;
            }

            cache.AllVertices[currentVertex] = properties;

            var allEdges = new LinkedList<PropertyEdge>();
            allEdges.AddRange(currentVertex.Children);
            currentVertex.Casts.ForEach(
                castEdge =>
                    {
                        if (!_typeGraph.PathExists(castEdge.Value.CastTo, searchVertex))
                        {
                            return;
                        }

                        allEdges.AddRange(castEdge.Value.CastTo.Children);
                    });

            allEdges.ForEach(
                searchNode =>
                {
                    var childVertex = searchNode.Value.Child;
                    var propertyInfo = searchNode.Value.PropertyInfo;

                    if (!_typeGraph.PathExists(childVertex, searchVertex))
                    {
                        return;
                    }

                    PropertyInfoVertex rootProperty;
                    if (!cache.AllProperties.TryGetValue(propertyInfo, out rootProperty))
                    {
                        rootProperty = new PropertyInfoVertex(propertyInfo);
                        cache.AllProperties[propertyInfo] = rootProperty;
                    }

                    properties.Add(rootProperty);
                });

            allEdges.ForEach(
                searchNode =>
                {
                    var childVertex = searchNode.Value.Child;
                    var propertyInfo = searchNode.Value.PropertyInfo;

                    PropertyInfoVertex rootProperty;
                    if (!cache.AllProperties.TryGetValue(propertyInfo, out rootProperty))
                    {
                        return;
                    }

                    var childProperties = BuildSearchTree(childVertex, searchVertex, cache);

                    rootProperty.Children.UnionWith(childProperties);

                    foreach (var childProperty in childProperties)
                    {
                        childProperty.Parents.Add(rootProperty);
                    }
                });

            return properties;
        }
    }
    
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

    public class CompositPropertyVertex
    {
        public LinkedList<PropertyInfo> PropertyList { get; } = new LinkedList<PropertyInfo>();

        public LinkedList<CompositPropertyVertex> Children { get; } = new LinkedList<CompositPropertyVertex>();

        public override string ToString()
        {
            return string.Join("->", PropertyList.Select(p => $"{p.DeclaringType.Name}.{p.Name}"));
        }
    }
}
