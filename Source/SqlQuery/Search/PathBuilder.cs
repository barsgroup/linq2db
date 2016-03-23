namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    using LinqToDB.Extensions;

    public class PathBuilderSearchCache
    {
        public Type SourceType { get; }

        public Type SearchType { get; }

        public Dictionary<TypeVertex, LinkedList<PropertyInfoVertex>> AllVertices { get; } = new Dictionary<TypeVertex, LinkedList<PropertyInfoVertex>>();

        public Dictionary<PropertyInfo, PropertyInfoVertex> AllProperties { get; } = new Dictionary<PropertyInfo, PropertyInfoVertex>();
        public Dictionary<PropertyInfo, CompositPropertyVertex> AllCompositProperties { get; } = new Dictionary<PropertyInfo, CompositPropertyVertex>();

        public Dictionary<PropertyInfo, List<Edge>> EdgeSubTree { get; set; }

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

            var sourceTypes = SearchHelper<TBaseSearchInterface>.FindInterfacesWithSelf(sourceType);

            var paths = GetOrBuildPaths(sourceTypes, searchType, cachedResult);
            var optimized = OptimizePaths(paths, cachedResult);

            cachedResult.OptimizedPaths = optimized;

            return optimized;
        }

        public LinkedList<PropertyInfoVertex> GetOrBuildPaths(IEnumerable<Type> sourceTypes, Type searchType, PathBuilderSearchCache cache)
        {
            var propertyPaths = new LinkedList<PropertyInfoVertex>();
            var searchVertex = _typeGraph.GetTypeVertex(searchType);

            foreach (var sourceType in sourceTypes)
            {
                var sourceVertex = _typeGraph.GetTypeVertex(sourceType);

                if (cache.AllVertices.ContainsKey(sourceVertex))
                {
                    continue;
                }

                if (!_typeGraph.ExtendedPathExists(sourceVertex, searchVertex))
                {
                    continue;
                }

                var isCycleStartVertex = new bool[_typeGraph.VertextCount];

                var properties = BuildSearchTree(sourceVertex, searchVertex, cache, new bool[_typeGraph.VertextCount], isCycleStartVertex);

                var cycleStartVertices = isCycleStartVertex.Select(
                    (flag, index) => new
                                         {
                                             flag,
                                             index
                                         }).Where(p => p.flag).Select(p => _typeGraph.GetTypeVertex(p.index)).ToList();

                foreach (var vertex in cycleStartVertices)
                {
                    var cycleStartProperties = cache.AllVertices[vertex];
                    cycleStartProperties.ForEach(node => node.Value.IsCycleStartVertex = true);
                }

                var cycleEndProperties = cache.AllProperties.Values.Where(p => p.IsCycleEndVertex);

                var testCycleStartVertexIndices = new HashSet<int>();

                foreach (var cycleEndProperty in cycleEndProperties)
                {
                    var propertyType = cycleEndProperty.Property.PropertyType;

                    var recursiveVertices = SearchHelper<TBaseSearchInterface>.FindInterfacesWithSelf(propertyType).Select(t => _typeGraph.GetTypeVertex(t));

                    foreach (var recursiveVertex in recursiveVertices)
                    {
                        if (!isCycleStartVertex[recursiveVertex.Index])
                        {
                            throw new Exception("Bad cycle (attempt to add recursive properties without cycle start flag)");
                        }

                        testCycleStartVertexIndices.Add(recursiveVertex.Index);

                        var recursiveChildren = cache.AllVertices[recursiveVertex];
                        recursiveChildren.ForEach(
                            recursiveChild =>
                                {
                                    recursiveChild.Value.ParentSet.Add(cycleEndProperty);
                                });

                        cycleEndProperty.ChildrenSet.AddRange(recursiveChildren);
                    }
                }

                if (cycleStartVertices.Select(v => v.Index).Except(testCycleStartVertexIndices).Any())
                {
                    throw new Exception("Bad cycle (some properties with cycle start flag are unused)");
                }

                if (properties.Count == 0)
                {
                    throw new InvalidOperationException("Не найден ни один путь");
                }
                
                propertyPaths.AddRange(properties);
            }

            if (propertyPaths.Count == 0)
            {
                throw new InvalidOperationException("Не найден ни один путь");
            }

            foreach (var propertyInfoVertex in cache.AllProperties)
            {
                propertyInfoVertex.Value.Build();
            }

            return propertyPaths;
        }

        public LinkedList<PropertyInfoVertex> GetOrBuildPathsNew(IEnumerable<Type> sourceTypes, Type searchType, PathBuilderSearchCache cache)
        {
            cache.EdgeSubTree = _typeGraph.GetEdgeSubTree(sourceTypes, searchType);

            var allEdges = cache.EdgeSubTree.Values.SelectMany(v => v).ToList();
            var edgesGroupedByParent = allEdges.GroupBy(v => v.Parent).ToDictionary(g => g.Key, g => g.ToList());
            var edgesGroupedByChild = allEdges.GroupBy(v => v.Child).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var edgeGroup in cache.EdgeSubTree)
            {
                cache.AllProperties[edgeGroup.Key] = new PropertyInfoVertex(edgeGroup.Key);
            }

            foreach (var edgeGroup in cache.EdgeSubTree)
            {
                var vertex = cache.AllProperties[edgeGroup.Key];

                foreach (var edge in edgeGroup.Value)
                {
                    LinkedList<PropertyInfoVertex> childEdges;
                    if (!cache.AllVertices.TryGetValue(edge.Child, out childEdges))
                    {
                        childEdges = new LinkedList<PropertyInfoVertex>();
                        cache.AllVertices[edge.Child] = childEdges;

                        var childEdgesList = edgesGroupedByParent[edge.Child].Select(e => cache.AllProperties[e.PropertyInfo]).Distinct();
                        childEdges.AddRange(childEdgesList);
                    }

                    vertex.ChildrenSet.AddRange(childEdges);

                    LinkedList<PropertyInfoVertex> parentEdges;
                    if (!cache.AllVertices.TryGetValue(edge.Parent, out parentEdges))
                    {
                        parentEdges = new LinkedList<PropertyInfoVertex>();
                        cache.AllVertices[edge.Parent] = parentEdges;

                        var parentEdgesList = edgesGroupedByChild[edge.Parent].Select(e => cache.AllProperties[e.PropertyInfo]).Distinct();
                        parentEdges.AddRange(parentEdgesList);
                    }

                    vertex.ParentSet.AddRange(parentEdges);

                    vertex.Build();
                }
            }

            var result = new LinkedList<PropertyInfoVertex>();
            var startEdges = sourceTypes.SelectMany(t => cache.AllVertices[_typeGraph.GetTypeVertex(t)]).Distinct();
            result.AddRange(startEdges);

            return result;
        }

        public static LinkedList<CompositPropertyVertex> OptimizePaths(LinkedList<PropertyInfoVertex> propertyPaths, PathBuilderSearchCache cache)
        {
            var optimizePaths = new LinkedList<CompositPropertyVertex>();

            propertyPaths.ForEach(node => optimizePaths.AddLast(OptimizeNode(node.Value, cache)));

            return optimizePaths;
        }

        //public static LinkedList<PropertyInfoVertex> Simplify(LinkedList<PropertyInfoVertex> original)
        //{
        //    var dictionary = new Dictionary<PropertyInfo, LinkedList<PropertyInfoVertex>>();
        //
        //    original.ForEach(
        //        node =>
        //        {
        //            var property = node.Value.Property;
        //            if (!dictionary.ContainsKey(property))
        //            {
        //                dictionary[property] = new LinkedList<PropertyInfoVertex>();
        //            }
        //            dictionary[property].AddLast(node.Value);
        //        });
        //
        //    var result = new LinkedList<PropertyInfoVertex>();
        //
        //    foreach (var sameNodes in dictionary)
        //    {
        //        var fullChildList = new LinkedList<PropertyInfoVertex>();
        //
        //        var mergedNode = new PropertyInfoVertex(sameNodes.Key);
        //
        //        sameNodes.Value.ForEach(
        //            sameNode =>
        //                {
        //                    mergedNode.IsCycleStartVertex = mergedNode.IsCycleStartVertex || sameNode.Value.IsCycleStartVertex;
        //                    mergedNode.IsCycleEndVertex = mergedNode.IsCycleStartVertex || sameNode.Value.IsCycleEndVertex;
        //
        //                    fullChildList.AddRange(sameNode.Value.Children);
        //                });
        //
        //        var simplifiedChildList = Simplify(fullChildList);
        //
        //        mergedNode.Children.AddRange(simplifiedChildList);
        //        result.AddLast(mergedNode);
        //    }
        //
        //    return result;
        //}

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

            while (current.Children.Count == 1)
            {
                var next = current.Children.First.Value;

                if (next.IsCycleStartVertex || next.ParentCount > 1 || cache.AllCompositProperties.ContainsKey(next.Property))
                {
                    break;
                }

                current = next;

                composite.PropertyList.AddLast(current.Property);
            }

            current.Children.ForEach(
                listNode =>
                    {
                        composite.Children.AddLast(OptimizeNode(listNode.Value, cache));
                    });
            
            return composite;
        }

        public LinkedList<PropertyInfoVertex> BuildSearchTree(
            TypeVertex currentVertex,
            TypeVertex searchVertex,
            PathBuilderSearchCache cache,
            bool[] visited, bool[] isCycleStartVertex)
        {
            var properties = new LinkedList<PropertyInfoVertex>();
            //if (visited[currentVertex.Index])
            //{
            //    isCycleStartVertex[currentVertex.Index] = true;
            //    return properties;
            //}
            
            //if (_typeGraph.IsFinalVertex(currentVertex, searchVertex.Type) && visited.Any(v => v))
            //{
            //    return properties;
            //}

            LinkedList<PropertyInfoVertex> cachedProperties;
            if (cache.AllVertices.TryGetValue(currentVertex, out cachedProperties))
            {
                return cachedProperties;
            }

            visited[currentVertex.Index] = true;

            currentVertex.Children.ForEach(
                searchNode =>
                {
                    var childVertex = searchNode.Value.Child;
                    var propertyInfo = searchNode.Value.PropertyInfo;

                    if (!_typeGraph.ExtendedPathExists(childVertex, searchVertex))
                    {
                        return;
                    }

                    PropertyInfoVertex rootProperty;
                    if (!cache.AllProperties.TryGetValue(propertyInfo, out rootProperty))
                    {
                        rootProperty = new PropertyInfoVertex(propertyInfo);
                        cache.AllProperties[propertyInfo] = rootProperty;
                    }

                    var childProperties = BuildSearchTree(childVertex, searchVertex, cache, visited, isCycleStartVertex);

                    if (childProperties.Count == 0 && isCycleStartVertex[childVertex.Index])
                    {
                        rootProperty.IsCycleEndVertex = true;
                    }
                    else
                    {
                        rootProperty.ChildrenSet.AddRange(childProperties);

                        childProperties.ForEach(
                            node =>
                                {
                                    node.Value.ParentSet.Add(rootProperty);
                                });
                    }

                    properties.AddLast(rootProperty);
                });

            visited[currentVertex.Index] = false;

            cache.AllVertices[currentVertex] = properties;
            return properties;
        }
    }

    [DebuggerDisplay("{Property}")]
    public class PropertyInfoVertex
    {
        public PropertyInfo Property { get; }
        
        public bool IsCycleStartVertex { get; set; }
        public bool IsCycleEndVertex { get; set; }

        public HashSet<PropertyInfoVertex> ParentSet { get; private set; } = new HashSet<PropertyInfoVertex>();
        public int ParentCount { get; private set; }

        public HashSet<PropertyInfoVertex> ChildrenSet { get; private set; } = new HashSet<PropertyInfoVertex>();
        public LinkedList<PropertyInfoVertex> Children { get; private set; }

        public PropertyInfoVertex(PropertyInfo property)
        {
            Property = property;
        }

        public void Build()
        {
            Children = new LinkedList<PropertyInfoVertex>();

            foreach (var propertyInfoVertex in ChildrenSet)
            {
                Children.AddLast(propertyInfoVertex);
            }

            ChildrenSet = null;

            ParentCount = ParentSet.Count;
            ParentSet = null;
        }
    }

    public class CompositPropertyVertex
    {
        public LinkedList<PropertyInfo> PropertyList { get; } = new LinkedList<PropertyInfo>();

        public LinkedList<CompositPropertyVertex> Children { get; } = new LinkedList<CompositPropertyVertex>();
    }
}
