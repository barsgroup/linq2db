﻿namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
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

            var rootDelegate = new RootProxyDelegate(delegates);

            return rootDelegate.Execute;
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
                        if (typeof(ICollection).IsAssignableFrom(node.Value.PropertyType))
                        {
                            hasCollection = true;
                        }

                        var castAs = Expression.TypeAs(parameter, node.Value.DeclaringType);
                        var checkNotNull = Expression.NotEqual(castAs, nullConst);
                        var memberAccess = Expression.Convert(Expression.MakeMemberAccess(castAs, node.Value), typeof(object));
                        var conditionalMemberAccess = Expression.Condition(checkNotNull, memberAccess, nullConst);

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
            protected readonly Func<object, object>[] PropertyGetters;

            protected readonly ProxyDelegate[] Children;

            protected ProxyDelegate(Func<object, object>[] propertyGetters, ProxyDelegate[] children)
            {
                PropertyGetters = propertyGetters;
                Children = children;
            }

            public static ProxyDelegate Create(Func<object, object>[] propertyGetters, ProxyDelegate[] children, bool hasCollection)
            {
                if (hasCollection)
                {
                    return new CollectionProxyDelegate(propertyGetters, children);
                }

                return new ScalarProxyDelegate(propertyGetters, children);
            }

            public abstract void Execute(object obj, LinkedList<TSearch> resultList, bool stepIntoFound, HashSet<object> visited = null);

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

            protected void HandleFinalPropertyValues(object source, int index, LinkedList<TSearch> resultList, bool stepIntoFound, HashSet<object> visited)
            {
                while (true)
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

                    if (nextObj is ICollection)
                    {
                        var colItems = CollectionUtils.GetCollectionItem(nextObj);
                        for (var i = 0; i < colItems.Length; i++)
                        {
                            if (colItems[i] == null)
                            {
                                continue;
                            }

                            HandleFinalPropertyValues(colItems[i], nextIndex, resultList, stepIntoFound, visited);
                        }
                    }
                    else
                    {
                        source = nextObj;
                        index = nextIndex;
                        continue;
                    }

                    break;
                }
            }
        }

        public sealed class ScalarProxyDelegate : ProxyDelegate
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

        public sealed class CollectionProxyDelegate : ProxyDelegate
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

        public class RootProxyDelegate : ProxyDelegate
        {
            public RootProxyDelegate(ProxyDelegate[] children)
                : base(null, children)
            {
            }

            public sealed override void Execute(object obj, LinkedList<TSearch> resultList, bool stepIntoFound, HashSet<object> visited = null)
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

                for (var i = 0; i < Children.Length; ++i)
                {
                    Children[i].Execute(obj, resultList, stepIntoFound, visited);
                }
            }
        }
    }
}