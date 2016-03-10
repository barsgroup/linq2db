namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public class SearchEngine
    {
        protected readonly Dictionary<Type, SearchVertex> SearchVertices = new Dictionary<Type, SearchVertex>(); 
        private readonly Dictionary<Type, InterfaceVertex> _interfacesVertices = new Dictionary<Type, InterfaceVertex>(); 

        private readonly Dictionary<Tuple<Type,Type>, LinkedList<Func<IQueryElement, IQueryElement>>> _searchPaths = new Dictionary<Tuple<Type, Type>, LinkedList<Func<IQueryElement, IQueryElement>>>();

        protected SearchEngine()
        {
            InitSearchVertex();
            InitInterfaceVertex();
        }

        public static SearchEngine Current { get; } = new SearchEngine();

        public void Find<TElement>(IQueryElement source)
        {
            Find(source, typeof(TElement));
        }

        public void Find(IQueryElement source, Type searchType)
        {
            var sourceType = GetSuitableInterface(source.GetType());

            var paths = GetOrBuildPaths(sourceType, searchType);
        }

        private LinkedList<Func<IQueryElement, IQueryElement>> GetOrBuildPaths(Type sourceType, Type searchType)
        {
            LinkedList<Func<IQueryElement, IQueryElement>> paths;

            var dictionaryKey = Tuple.Create(sourceType, searchType);

            if (_searchPaths.TryGetValue(dictionaryKey, out paths))
                return paths;
            
            paths = _searchPaths[dictionaryKey] = new LinkedList<Func<IQueryElement, IQueryElement>>();

            var sourceVertex = SearchVertices[sourceType];

            var allVertex = new Dictionary<PropertyInfo, PropertyPath>();
            var visitedProperties = new HashSet<PropertyInfo>();
            var properties = GetFindTree(sourceVertex, searchType, sourceType, allVertex, visitedProperties);

            if (properties.Count == 0)
            {
                throw new InvalidOperationException("Не найден ни один путь");
            }

            OptimizePaths(properties);

            return paths;
        }


        protected Dictionary<PropertyInfo, LinkedList<PropertyInfo>> OptimizePaths(LinkedList<PropertyPath> source)
        {
            var resultPaths = new LinkedList<LinkedList<PropertyPath>>();

            Walk(source,
             (propertyPath, typePath, path) =>
             {
                 if (propertyPath.Types.Count == 0 ||  typePath != null && (typePath.PropertyPaths.Count > 1 || typePath.PropertyPaths.Count == 0))
                 {
                     resultPaths.AddLast(new LinkedList<PropertyPath>(path));
                 }

                 if (propertyPath.Types.Count == 0 ||  typePath != null && typePath.PropertyPaths.Count > 1)
                 {
                     var last = path.Last.Value;
                     path.Clear();
                     path.AddLast(last);
                 }
             });

            return null;
        }


        [DebuggerHidden]
        private void Walk(IEnumerable<PropertyPath> source, Action<PropertyPath, TypePath, LinkedList<PropertyPath>> action, LinkedList<PropertyPath> path = null)
        {
            if (path == null)
            {
                path = new LinkedList<PropertyPath>();
            }

            foreach (var propertyPath in source)
            {
                path.AddLast(propertyPath);
                action(propertyPath, null, path);

                foreach (var typePath in propertyPath.Types)
                {
                    action(propertyPath, typePath, path);
                    Walk(typePath.PropertyPaths, action, path);
                }

                if (path.First != null)
                {
                    path.RemoveLast();
                }
            }
        }

        protected LinkedList<PropertyPath> GetFindTree(SearchVertex currentVertex, Type searchType, Type baseType, Dictionary<PropertyInfo, PropertyPath> allProperties, ISet<PropertyInfo> visitedProperties  )
        {
            var properties = new LinkedList<PropertyPath>();

            if (currentVertex.Type == baseType && visitedProperties.Count > 0 || searchType.IsAssignableFrom(currentVertex.Type))
            {
                return properties;
            }

            currentVertex.Children.ForEach(
                searchNode =>
                {
                    var type = searchNode.Value.Item2.Type;

                    var propertyInfo = searchNode.Value.Item1;

                    if (visitedProperties.Contains(propertyInfo))
                    {
                        return;
                    }

                    PropertyPath propPath;
                    if (allProperties.TryGetValue(propertyInfo, out propPath))
                    {
                        properties.AddLast(propPath);
                        return;
                    }

                    propPath = new PropertyPath(propertyInfo);
                    allProperties[propPath.Property] = propPath;

                    if (type == baseType || searchType.IsAssignableFrom(type))
                    {
                        properties.AddLast(propPath);
                        return;
                    }

                    visitedProperties.Add(propertyInfo);

                    var interfaceVertex = _interfacesVertices[type];

                    interfaceVertex.Children.ForEach(
                        branchNode =>
                        {
                            var findedPaths = GetFindTree(SearchVertices[branchNode.Value.Type], searchType, baseType, allProperties, visitedProperties);
                            var childTypePath = new TypePath(branchNode.Value.Type)
                                                {
                                                    PropertyPaths = findedPaths
                                                };

                            if (childTypePath.PropertyPaths.Count > 0 || branchNode.Value.Type == baseType || searchType.IsAssignableFrom(branchNode.Value.Type))
                            {
                                propPath.Types.AddLast(childTypePath);
                            }

                        });

                    if (propPath.Types.Count > 0)
                    {
                        properties.AddLast(propPath);
                    }
                    else
                    {
                        allProperties.Remove(propPath.Property);
                    }
                });

            return properties;
        }

        private void InitSearchVertex()
        {
            var types = typeof(SearchVertex).Assembly.GetTypes().Where(t => typeof(IQueryElement).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            foreach (var type in types)
            {
                var intType = GetSuitableInterface(type);
                var propertyInfos = GetPublicProperties(intType).Where(p => p.GetCustomAttribute<SearchContainerAttribute>() != null);

                SearchVertex vertex;

                if (!SearchVertices.TryGetValue(intType, out vertex))
                {
                     vertex = SearchVertices[intType] = new SearchVertex(intType);
                }

                foreach (var info in propertyInfos)
                {
                    var propertyType = GetElementType(info.PropertyType);
                     
                    if (!typeof(IQueryElement).IsAssignableFrom(propertyType) && !IsCollectionType(info.PropertyType))
                    {
                        throw new InvalidCastException();
                    }

                    SearchVertex childVertex;
                    if (!SearchVertices.TryGetValue(propertyType, out childVertex)) 
                    {
                        childVertex = SearchVertices[propertyType] = new SearchVertex(propertyType);
                    }

                    vertex.Children.AddLast(Tuple.Create(info, childVertex));
                }

            }
        }

        private void InitInterfaceVertex()
        {
            var interfaces = typeof(IQueryElement).Assembly.GetTypes().Where(t => t.IsInterface && typeof(IQueryElement).IsAssignableFrom(t));

            foreach (var inter in interfaces)
            {
                InterfaceVertex vertex;
                if (!_interfacesVertices.TryGetValue(inter, out vertex))
                {
                    vertex = _interfacesVertices[inter] = new InterfaceVertex(inter);
                }

                var childInterfaces = inter.GetInterfaces().Where(typeof(IQueryElement).IsAssignableFrom).Concat(new [] { inter });

                foreach (var childInterface in childInterfaces)
                {
                    InterfaceVertex childVertex;
                    if (!_interfacesVertices.TryGetValue(childInterface, out childVertex))
                    {
                        childVertex = _interfacesVertices[childInterface] = new InterfaceVertex(childInterface);
                    }

                    childVertex.Children.AddLast(vertex);
                }


                var forDeleted = new LinkedList<InterfaceVertex>();
                vertex.Children.ForEach(
                    listNode =>
                    {

                        vertex.Children.ForEach(
                            node =>
                            {
                                if (listNode == node)
                                {
                                    return;
                                }

                                if (listNode.Value.Type.IsAssignableFrom(node.Value.Type))
                                {
                                    forDeleted.AddLast(node.Value);
                                }
                            });

                    });
                forDeleted.ForEach(node => vertex.Children.Remove(node.Value));
            }
        }


        private Type GetSuitableInterface(Type type)
        {
            var list = type.GetInterfaces().Where(typeof(IQueryElement).IsAssignableFrom).ToList();

            Type resultType = null;
            for (var i = 0; i < list.Count; i++)
            {
                var j = 0;
                while (i == j || j < list.Count && !list[i].IsAssignableFrom(list[j]))
                {
                    j++;
                }

                if (j == list.Count)
                {
                    if (resultType == null)
                    {
                        resultType = list[i];
                    }
                    else
                    {
                        throw new InvalidOperationException("Обнаружено несколько рутовых интерфейсов");
                    }
                }
            }

            if (resultType != null)
            {
                return resultType;
            }

            throw new InvalidOperationException("Все типы зависят друг от друга");
        }


        public static PropertyInfo[] GetPublicProperties(Type type)
        {
            if (type.IsInterface)
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    var subType = queue.Dequeue();
                    foreach (var subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface)) continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    var typeProperties = subType.GetProperties(
                        BindingFlags.FlattenHierarchy
                        | BindingFlags.Public
                        | BindingFlags.Instance);

                    var newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

            return type.GetProperties(BindingFlags.FlattenHierarchy
                | BindingFlags.Public | BindingFlags.Instance);
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

            if (typeof(Array).IsAssignableFrom(sourceType.GetGenericTypeDefinition()))
            {
                return sourceType.GetGenericArguments()[0];
            }

            return sourceType;

        }

        private static bool IsCollectionType(Type sourceType)
        {
            return GetElementType(sourceType) != sourceType;
        }

        private class InterfaceVertex
        {
            public Type Type { get; }

            public LinkedList<InterfaceVertex> Children { get; } = new LinkedList<InterfaceVertex>();

            public InterfaceVertex(Type type)
            {
                Type = type;
            }
        }

        protected class SearchVertex
        {
            public Type Type { get; }

            public LinkedList<Tuple<PropertyInfo, SearchVertex>> Children { get; } = new LinkedList<Tuple<PropertyInfo, SearchVertex>>();

            public SearchVertex(Type type)
            {
                Type = type;
            }
        }
    }
    [DebuggerDisplay("{Type}")]
    public class TypePath
    {
        public Type Type { get; }

        public LinkedList<PropertyPath> PropertyPaths { get; set; }

        public TypePath(Type type)
        {
            Type = type;
        }

    }

    [DebuggerDisplay("{Property}")]
    public class PropertyPath
    {
        public PropertyInfo Property { get; }

        public LinkedList<TypePath> Types { get;} = new LinkedList<TypePath>();

        public PropertyPath(PropertyInfo property)
        {
            Property = property;
        }
    }
}