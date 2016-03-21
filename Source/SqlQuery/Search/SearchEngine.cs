namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    using LinqToDB.Extensions;

    public class SearchEngine<TBaseSearchInterface>
    {
        private TypeGraph<TBaseSearchInterface> _typeGraph;

        protected SearchEngine()
        {
            InitSearchVertex();
        }

        public static SearchEngine<TBaseSearchInterface> Current { get; } = new SearchEngine<TBaseSearchInterface>();

        public void Find<TElement>(TBaseSearchInterface source)
        {
            Find(source, typeof(TElement));
        }

        public void Find(TBaseSearchInterface source, Type searchType)
        {
            var sourceTypes = FindInterfacesWithSelf(source.GetType());

            var paths = GetOrBuildPaths(sourceTypes, searchType);
        }

        private LinkedList<Func<TBaseSearchInterface, TBaseSearchInterface>> GetOrBuildPaths(IEnumerable<Type> sourceTypes, Type searchType)
        {
            var resultPaths = new LinkedList<Func<TBaseSearchInterface, TBaseSearchInterface>>();
            var propertyPaths = new LinkedList<PropertyInfoVertex>();
            var searchVertex = _typeGraph.SearchVertices[searchType];
            var allVertex = new Dictionary<TypeVertex, LinkedList<PropertyInfoVertex>>();

            foreach (var sourceType in sourceTypes)
            {
                var sourceVertex = _typeGraph.SearchVertices[sourceType];

                if (!_typeGraph.PathExists(sourceVertex, searchVertex))
                {
                    continue;
                }

                var properties = BuildSearchTree(sourceVertex, searchVertex, allVertex, new bool[TypeVertex.Counter]);

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

            var optimizePaths = new LinkedList<CompositPropertyVertex>();

            propertyPaths.ForEach(node => optimizePaths.AddLast(OptimizeNode(node.Value)));

            //var linearized = new LinkedList<LinkedList<CompositPropertyVertex>>();
            //optimizePaths.ForEach(node =>
            //{
            //    linearized.AddRange(Linearize(node.Value));
            //});

            return resultPaths;
        }

        //private LinkedList<LinkedList<CompositPropertyVertex>> Linearize(CompositPropertyVertex node)
        //{
        //    var results = new LinkedList<LinkedList<CompositPropertyVertex>>();
        //
        //    if (node.Children.Count == 0)
        //    {
        //        var list = new LinkedList<CompositPropertyVertex>();
        //        list.AddLast(node);
        //        results.AddLast(list);
        //        return results;
        //    }
        //
        //    node.Children.ForEach(
        //        child =>
        //        {
        //            var childResults = Linearize(child.Value);
        //            childResults.ForEach(
        //                childResult =>
        //                {
        //                    var list = new LinkedList<CompositPropertyVertex>();
        //                    list.AddLast(node);
        //                    list.AddRange(childResult.Value);
        //                    results.AddLast(list);
        //
        //                });
        //        });
        //
        //    return results;
        //}

        private LinkedList<PropertyInfoVertex> Simplify(LinkedList<PropertyInfoVertex> original)
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


        //protected Dictionary<PropertyInfo, LinkedList<PropertyInfo>> OptimizePaths(LinkedList<PropertyInfoVertex> source)
        //{
        //    var resultPaths = new LinkedList<LinkedList<PropertyInfoVertex>>();

        //    Walk(source,
        //     (propertyPath, typePath, path) =>
        //     {
        //         if (propertyPath.Types.Count == 0 ||  typePath != null && (typePath.PropertyPaths.Count > 1 || typePath.PropertyPaths.Count == 0))
        //         {
        //             resultPaths.AddLast(new LinkedList<PropertyInfoVertex>(path));
        //         }

        //         if (propertyPath.Types.Count == 0 ||  typePath != null && typePath.PropertyPaths.Count > 1)
        //         {
        //             var last = path.Last.Value;
        //             path.Clear();
        //             path.AddLast(last);
        //         }
        //     });

        //    return null;
        //}


        //[DebuggerHidden]
        //private void Walk(IEnumerable<PropertyInfoVertex> source, Action<PropertyInfoVertex, TypePath, LinkedList<PropertyInfoVertex>> action, LinkedList<PropertyInfoVertex> path = null)
        //{
        //    if (path == null)
        //    {
        //        path = new LinkedList<PropertyInfoVertex>();
        //    }

        //    foreach (var propertyPath in source)
        //    {
        //        path.AddLast(propertyPath);
        //        action(propertyPath, null, path);

        //        foreach (var typePath in propertyPath.Types)
        //        {
        //            action(propertyPath, typePath, path);
        //            Walk(typePath.PropertyPaths, action, path);
        //        }

        //        if (path.First != null)
        //        {
        //            path.RemoveLast();
        //        }
        //    }
        //}

        private LinkedList<PropertyInfoVertex> BuildSearchTree(TypeVertex currentVertex, TypeVertex searchVertex, Dictionary<TypeVertex, LinkedList<PropertyInfoVertex>> allProperties, bool[] visited)
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

                    var childProperties = BuildSearchTree(_typeGraph.SearchVertices[typeVertex.Type], searchVertex, allProperties, visited);

                    var rootProperty = new PropertyInfoVertex(propertyInfo);
                    rootProperty.Children.AddRange(childProperties);
                    properties.AddLast(rootProperty);
                });

            visited[currentVertex.Index] = false;

            allProperties[currentVertex] = properties;
            return properties;
        }

        private void InitSearchVertex()
        {
            var types = typeof(TypeVertex).Assembly.GetTypes();
            _typeGraph = new TypeGraph<TBaseSearchInterface>(types);
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

        //private void InitInterfaceVertex()
        //{
        //    var interfaces = typeof(TBaseSearchInterface).Assembly.GetTypes().Where(t => t.IsInterface && typeof(TBaseSearchInterface).IsAssignableFrom(t));

        //    foreach (var inter in interfaces)
        //    {
        //        InterfaceVertex vertex;
        //        if (!_interfacesVertices.TryGetValue(inter, out vertex))
        //        {
        //            vertex = _interfacesVertices[inter] = new InterfaceVertex(inter);
        //        }

        //        var childInterfaces = inter.GetInterfaces().Where(typeof(TBaseSearchInterface).IsAssignableFrom).Concat(new [] { inter });

        //        foreach (var childInterface in childInterfaces)
        //        {
        //            InterfaceVertex childVertex;
        //            if (!_interfacesVertices.TryGetValue(childInterface, out childVertex))
        //            {
        //                childVertex = _interfacesVertices[childInterface] = new InterfaceVertex(childInterface);
        //            }

        //            childVertex.Children.AddLast(vertex);
        //        }


        //        var forDeleted = new LinkedList<InterfaceVertex>();
        //        vertex.Children.ForEach(
        //            listNode =>
        //            {

        //                vertex.Children.ForEach(
        //                    node =>
        //                    {
        //                        if (listNode == node)
        //                        {
        //                            return;
        //                        }

        //                        if (listNode.Value.Type.IsAssignableFrom(node.Value.Type))
        //                        {
        //                            forDeleted.AddLast(node.Value);
        //                        }
        //                    });

        //            });
        //        forDeleted.ForEach(node => vertex.Children.Remove(node.Value));
        //    }
        //}


        //private Type GetSuitableInterface(Type type)
        //{
        //    var list = type.GetInterfaces().Where(typeof(TBaseSearchInterface).IsAssignableFrom).ToList();

        //    Type resultType = null;
        //    for (var i = 0; i < list.Count; i++)
        //    {
        //        var j = 0;
        //        while (i == j || j < list.Count && !list[i].IsAssignableFrom(list[j]))
        //        {
        //            j++;
        //        }

        //        if (j == list.Count)
        //        {
        //            if (resultType == null)
        //            {
        //                resultType = list[i];
        //            }
        //            else
        //            {
        //                throw new InvalidOperationException("Обнаружено несколько рутовых интерфейсов");
        //            }
        //        }
        //    }

        //    if (resultType != null)
        //    {
        //        return resultType;
        //    }

        //    throw new InvalidOperationException("Все типы зависят друг от друга");
        //}


       // public static PropertyInfo[] GetPublicProperties(Type type)
       // {
       //     if (type.IsInterface)
       //     {
       //         var propertyInfos = new List<PropertyInfo>();
       //
       //         var considered = new List<Type>();
       //         var queue = new Queue<Type>();
       //         considered.Add(type);
       //         queue.Enqueue(type);
       //         while (queue.Count > 0)
       //         {
       //             var subType = queue.Dequeue();
       //             foreach (var subInterface in subType.GetInterfaces())
       //             {
       //                 if (considered.Contains(subInterface)) continue;
       //
       //                 considered.Add(subInterface);
       //                 queue.Enqueue(subInterface);
       //             }
       //
       //             var typeProperties = subType.GetProperties(
       //                 BindingFlags.FlattenHierarchy
       //                 | BindingFlags.Public
       //                 | BindingFlags.Instance);
       //
       //             var newPropertyInfos = typeProperties
       //                 .Where(x => !propertyInfos.Contains(x));
       //
       //             propertyInfos.InsertRange(0, newPropertyInfos);
       //         }
       //
       //         return propertyInfos.ToArray();
       //     }
       //
       //     return type.GetProperties(BindingFlags.FlattenHierarchy
       //         | BindingFlags.Public | BindingFlags.Instance);
       // }

        //private class InterfaceVertex
        //{
        //    public Type Type { get; }

        //    public LinkedList<InterfaceVertex> Children { get; } = new LinkedList<InterfaceVertex>();

        //    public InterfaceVertex(Type type)
        //    {
        //        Type = type;
        //    }
        //}
    }

    //[DebuggerDisplay("{Type}")]
    //public class TypePath
    //{
    //    public Type Type { get; }

    //    public LinkedList<PropertyPath> PropertyPaths { get; set; }

    //    public TypePath(Type type)
    //    {
    //        Type = type;
    //    }

    //}

    [DebuggerDisplay("{Property}")]
    public class PropertyInfoVertex
    {
        public PropertyInfo Property { get; }

        public LinkedList<PropertyInfoVertex> Children { get;} = new LinkedList<PropertyInfoVertex>();

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