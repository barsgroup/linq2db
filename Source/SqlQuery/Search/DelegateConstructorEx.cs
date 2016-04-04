namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search.PathBuilder;
    using LinqToDB.SqlQuery.Search.Utils;

    public delegate void ResultDelegate<TSearch>(object obj, LinkedList<TSearch> resultList, bool stepIntoFound, HashSet<object> visited = null);

    public class DelegateConstructor<TSearch>
        where TSearch : class
    {
        public ResultDelegate<TSearch> CreateResultDelegate(LinkedList<CompositPropertyVertex> vertices)
        {
            var delegateMap = new Dictionary<CompositPropertyVertex, ProxyDelegate>();

            var delegates = new ProxyDelegate[vertices.Count];

            var index = 0;
            vertices.ForEach(
                node =>
                    {
                        CreateDelegate(node.Value, delegateMap);
                        delegates[index++] = delegateMap[node.Value];
                    });

            ResultDelegate<TSearch> findDelegate = (obj, resultList, stepIntoFound, visited) =>
                {
                    if (visited == null)
                    {
                        visited = new HashSet<object>();
                    }
            
                    if (obj == null || visited.Contains(obj))
                    {
                        return;
                    }
            
                    visited.Add(obj);
            
                    var searchObj = obj as TSearch;
                    if (searchObj != null && stepIntoFound)
                    {
                        resultList.AddLast(searchObj);
                    }

                    for (var i = 0; i < delegates.Length; ++i)
                    {
                        delegates[i].Execute(obj, resultList, stepIntoFound, visited);
                    }
                };

            return findDelegate;
        }

        private void CreateDelegate(CompositPropertyVertex vertex, Dictionary<CompositPropertyVertex, ProxyDelegate> delegateMap)
        {
            if (vertex.PropertyList.First == null)
            {
                throw new NullReferenceException();
            }

            if (delegateMap.ContainsKey(vertex))
            {
                return;
            }

            var propertyGetters = new Func<object, object>[vertex.PropertyList.Count];

            var parameter = Expression.Parameter(typeof(object), "obj");
            var nullConst = Expression.Constant(null);

            var hasCollection = false;

            var index = 0;
            vertex.PropertyList.ForEach(
                node =>
                    {
                        if (CollectionUtils.IsCollection(node.Value.PropertyType))
                        {
                            hasCollection = true;
                        }

                        var cast = Expression.Convert(parameter, node.Value.DeclaringType);
                        var memberAccess = Expression.Convert(Expression.Property(cast, node.Value.Name), typeof(object));
                        var checkType = Expression.TypeIs(parameter, node.Value.DeclaringType);
                        var conditionalMemberAccess = Expression.Condition(checkType, memberAccess, nullConst);

                        var deleg = Expression.Lambda<Func<object, object>>(conditionalMemberAccess, parameter).Compile();

                        propertyGetters[index++] = deleg;
                    });

            var childDelegates = new ProxyDelegate[vertex.Children.Count];

            delegateMap[vertex] = ProxyDelegate.Create(propertyGetters, childDelegates, hasCollection);

            index = 0;
            vertex.Children.ForEach(
                node =>
                    {
                        CreateDelegate(node.Value, delegateMap);
                        childDelegates[index++] = delegateMap[node.Value];
                    });
        }

        public abstract class ProxyDelegate
        {
            public Func<object, object>[] PropertyGetters { get; }

            public ProxyDelegate[] Children { get; }

            public ProxyDelegate(Func<object, object>[] propertyGetters, ProxyDelegate[] children)
            {
                PropertyGetters = propertyGetters;
                Children = children;
            }

            public abstract void Execute(object obj, LinkedList<TSearch> resultList, bool stepIntoFound, HashSet<object> visited = null);

            public static ProxyDelegate Create(Func<object, object>[] propertyGetters, ProxyDelegate[] children, bool hasCollection)
            {
                if (hasCollection)
                {
                    return new CollectionProxyDelegate(propertyGetters, children);
                }

                return new ScalarProxyDelegate(propertyGetters, children);
            }

            protected void HandleValue(object value, LinkedList<TSearch> resultList, bool stepIntoFound, HashSet<object> visited)
            {
                if (visited.Contains(value))
                {
                    return;
                }

                visited.Add(value);

                var searchValue = value as TSearch;
                if (searchValue != null)
                {
                    resultList.AddLast(searchValue);

                    if (!stepIntoFound)
                    {
                        return;
                    }
                }

                for (var i = 0; i < Children.Length; ++i)
                {
                    Children[i].Execute(value, resultList, stepIntoFound, visited);
                }
            }

            protected void HandleFinalPropertyValues(
                object source,
                int index,
                LinkedList<TSearch> resultList,
                bool stepIntoFound,
                HashSet<object> visited)
            {
                if (index == PropertyGetters.Length)
                {
                    HandleValue(source, resultList, stepIntoFound, visited);
                    return;
                }

                var nextObj = PropertyGetters[index](source);

                if (nextObj == null)
                {
                    return;
                }

                var nextIndex = index + 1;

                if (CollectionUtils.IsCollection(nextObj.GetType()))
                {
                    var colItems = CollectionUtils.GetCollectionItem(nextObj);
                    foreach (var colItem in colItems)
                    {
                        if (colItem == null)
                        {
                            continue;
                        }

                        HandleFinalPropertyValues(colItem, nextIndex, resultList, stepIntoFound, visited);
                    }
                }
                else
                {
                    HandleFinalPropertyValues(nextObj, nextIndex, resultList, stepIntoFound, visited);
                }
            }
        }

        public class ScalarProxyDelegate : ProxyDelegate
        {
            public ScalarProxyDelegate(Func<object, object>[] propertyGetters, ProxyDelegate[] children) : base(propertyGetters, children)
            {
            }

            public override void Execute(object obj, LinkedList<TSearch> resultList, bool stepIntoFound, HashSet<object> visited = null)
            {
                var currentObj = obj;

                for (var i = 0; i < PropertyGetters.Length; ++i)
                {
                    currentObj = PropertyGetters[i](currentObj);

                    if (currentObj == null)
                    {
                        return;
                    }
                }

                HandleValue(currentObj, resultList, stepIntoFound, visited);
            }
        }

        public class CollectionProxyDelegate : ProxyDelegate
        {
            public CollectionProxyDelegate(Func<object, object>[] propertyGetters, ProxyDelegate[] children)
                : base(propertyGetters, children)
            {
            }

            public override void Execute(object obj, LinkedList<TSearch> resultList, bool stepIntoFound, HashSet<object> visited = null)
            {
                HandleFinalPropertyValues(obj, 0, resultList, stepIntoFound, visited);
            }
        }
    }
}