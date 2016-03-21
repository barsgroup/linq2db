namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    using LinqToDB.Extensions;

    public class PathBuilder<TBaseSearchInterface>
    {
        private readonly TypeGraph<TBaseSearchInterface> _typeGraph;

        public PathBuilder(TypeGraph<TBaseSearchInterface> typeGraph)
        {
            _typeGraph = typeGraph;
        }

        public void Find(TBaseSearchInterface source, Type searchType)
        {
            var sourceTypes = source.GetType().FindInterfacesWithSelf<TBaseSearchInterface>();

            var paths = GetOrBuildPaths(sourceTypes, searchType);
            var optimized = OptimizePaths(paths);
        }

        public LinkedList<PropertyInfoVertex> GetOrBuildPaths(IEnumerable<Type> sourceTypes, Type searchType)
        {
            var propertyPaths = new LinkedList<PropertyInfoVertex>();
            var searchVertex = _typeGraph.GetTypeVertex(searchType);
            var allVertex = new Dictionary<TypeVertex, LinkedList<PropertyInfoVertex>>();

            foreach (var sourceType in sourceTypes)
            {
                var sourceVertex = _typeGraph.GetTypeVertex(sourceType);

                if (!_typeGraph.PathExists(sourceVertex, searchVertex))
                {
                    continue;
                }

                var properties = BuildSearchTree(sourceVertex, searchVertex, allVertex, new bool[_typeGraph.VertextCount]);

                if (properties.Count == 0)
                {
                    throw new InvalidOperationException("Не найден ни один путь");
                }

                properties = Simplify(properties);
                propertyPaths.AddRange(properties);
            }

            if (propertyPaths.Count == 0)
            {
                throw new InvalidOperationException("Не найден ни один путь");
            }

            propertyPaths = Simplify(propertyPaths);

            return propertyPaths;
        }

        public LinkedList<CompositPropertyVertex> OptimizePaths(LinkedList<PropertyInfoVertex> propertyPaths)
        {
            var optimizePaths = new LinkedList<CompositPropertyVertex>();

            propertyPaths.ForEach(node => optimizePaths.AddLast(OptimizeNode(node.Value)));

            return optimizePaths;
        }

        public LinkedList<PropertyInfoVertex> Simplify(LinkedList<PropertyInfoVertex> original)
        {
            var dictionary = new Dictionary<PropertyInfo, LinkedList<PropertyInfoVertex>>();

            original.ForEach(
                node =>
                {
                    var property = node.Value.Property;
                    if (!dictionary.ContainsKey(property))
                    {
                        dictionary[property] = new LinkedList<PropertyInfoVertex>();
                    }
                    dictionary[property].AddLast(node.Value);
                });

            var result = new LinkedList<PropertyInfoVertex>();

            foreach (var sameNodes in dictionary)
            {
                var fullChildList = new LinkedList<PropertyInfoVertex>();

                sameNodes.Value.ForEach(
                    sameNode =>
                    {
                        fullChildList.AddRange(sameNode.Value.Children);
                    });

                var simplifiedChildList = Simplify(fullChildList);

                var mergedNode = new PropertyInfoVertex(sameNodes.Key);

                mergedNode.Children.AddRange(simplifiedChildList);
                result.AddLast(mergedNode);
            }

            return result;
        }

        private CompositPropertyVertex OptimizeNode(PropertyInfoVertex node)
        {
            var composite = new CompositPropertyVertex();

            composite.PropertyList.AddLast(node.Property);

            var current = node;
            while (current.Children.Count == 1)
            {
                current = current.Children.First.Value;

                composite.PropertyList.AddLast(current.Property);
            }

            current.Children.ForEach(
                listNode =>
                {
                    composite.Children.AddLast(OptimizeNode(listNode.Value));
                });

            return composite;
        }

        public LinkedList<PropertyInfoVertex> BuildSearchTree(
            TypeVertex currentVertex,
            TypeVertex searchVertex,
            Dictionary<TypeVertex, LinkedList<PropertyInfoVertex>> allProperties,
            bool[] visited)
        {
            var properties = new LinkedList<PropertyInfoVertex>();
            if (visited[currentVertex.Index] || searchVertex.Type.IsAssignableFrom(currentVertex.Type) && visited.Any(v => v))
            {
                return properties;
            }

            LinkedList<PropertyInfoVertex> cachedProperties;
            if (allProperties.TryGetValue(currentVertex, out cachedProperties))
            {
                return cachedProperties;
            }

            visited[currentVertex.Index] = true;

            currentVertex.Children.ForEach(
                searchNode =>
                {
                    var typeVertex = searchNode.Value.Item2;
                    var propertyInfo = searchNode.Value.Item1;

                    if (!_typeGraph.PathExists(currentVertex, searchVertex))
                    {
                        return;
                    }

                    var childProperties = BuildSearchTree(_typeGraph.GetTypeVertex(typeVertex.Type), searchVertex, allProperties, visited);

                    var rootProperty = new PropertyInfoVertex(propertyInfo);
                    rootProperty.Children.AddRange(childProperties);
                    properties.AddLast(rootProperty);
                });

            visited[currentVertex.Index] = false;

            allProperties[currentVertex] = properties;
            return properties;
        }
    }

    [DebuggerDisplay("{Property}")]
    public class PropertyInfoVertex
    {
        public PropertyInfo Property { get; }
        
        public LinkedList<PropertyInfoVertex> Children { get; } = new LinkedList<PropertyInfoVertex>();

        public PropertyInfoVertex(PropertyInfo property)
        {
            Property = property;
        }
    }

    public class CompositPropertyVertex
    {
        public LinkedList<PropertyInfo> PropertyList { get; } = new LinkedList<PropertyInfo>();

        public LinkedList<CompositPropertyVertex> Children { get; } = new LinkedList<CompositPropertyVertex>();
    }
}
