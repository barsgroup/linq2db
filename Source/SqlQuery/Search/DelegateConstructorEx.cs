namespace LinqToDB.SqlQuery.Search
{
    using System;
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

            var delegates = new LinkedList<ProxyDelegate>();

            vertices.ForEach(
                node =>
                    {
                        CreateDelegate(node.Value, delegateMap);
                        delegates.AddLast(delegateMap[node.Value]);
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

                    delegates.ForEach(
                        node =>
                            {
                                node.Value.Execute(obj, resultList, stepIntoFound, visited);
                            });
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

            var propertyGetters = new LinkedList<Func<object, object>>();

            var parameter = Expression.Parameter(typeof(object), "obj");
            var nullConst = Expression.Constant(null);

            var hasCollection = false;

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

                        propertyGetters.AddLast(deleg);
                    });

            var childDelegates = new LinkedList<ProxyDelegate>();

            delegateMap[vertex] = ProxyDelegate.Create(propertyGetters, childDelegates, hasCollection);

            vertex.Children.ForEach(
                node =>
                    {
                        CreateDelegate(node.Value, delegateMap);
                        childDelegates.AddLast(delegateMap[node.Value]);
                    });
        }

        abstract class ProxyDelegate
        {
            public LinkedList<Func<object, object>> PropertyGetters { get; }

            public LinkedList<ProxyDelegate> Children { get; }

            public ProxyDelegate(LinkedList<Func<object, object>> propertyGetters, LinkedList<ProxyDelegate> children)
            {
                PropertyGetters = propertyGetters;
                Children = children;
            }

            public abstract void Execute(object obj, LinkedList<TSearch> resultList, bool stepIntoFound, HashSet<object> visited = null);

            public static ProxyDelegate Create(LinkedList<Func<object, object>> propertyGetters, LinkedList<ProxyDelegate> children, bool hasCollection)
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

                Children.ForEach(
                    childNode =>
                    {
                        childNode.Value.Execute(value, resultList, stepIntoFound, visited);
                    });
            }

            protected void HandleFinalPropertyValues(
                object source,
                LinkedListNode<Func<object, object>> propertyGetterNode,
                LinkedList<TSearch> resultList,
                bool stepIntoFound,
                HashSet<object> visited)
            {
                if (propertyGetterNode == null)
                {
                    HandleValue(source, resultList, stepIntoFound, visited);
                    return;
                }

                var nextObj = propertyGetterNode.Value(source);

                if (nextObj == null)
                {
                    return;
                }

                var nextGetterNode = propertyGetterNode.Next;

                if (CollectionUtils.IsCollection(nextObj.GetType()))
                {
                    var colItems = CollectionUtils.GetCollectionItem(nextObj);
                    foreach (var colItem in colItems)
                    {
                        if (colItem == null)
                        {
                            continue;
                        }

                        HandleFinalPropertyValues(colItem, nextGetterNode, resultList, stepIntoFound, visited);
                    }
                }
                else
                {
                    HandleFinalPropertyValues(nextObj, nextGetterNode, resultList, stepIntoFound, visited);
                }
            }
        }

        class ScalarProxyDelegate : ProxyDelegate
        {
            public ScalarProxyDelegate(LinkedList<Func<object, object>> propertyGetters, LinkedList<ProxyDelegate> children) : base(propertyGetters, children)
            {
            }

            public override void Execute(object obj, LinkedList<TSearch> resultList, bool stepIntoFound, HashSet<object> visited = null)
            {
                var currentObj = obj;
                var curDelegateNode = PropertyGetters.First;

                do
                {
                    currentObj = curDelegateNode.Value(currentObj);

                    if (currentObj == null)
                    {
                        return;
                    }

                    curDelegateNode = curDelegateNode.Next;
                }
                while (curDelegateNode != null);

                HandleValue(currentObj, resultList, stepIntoFound, visited);
            }
        }

        class CollectionProxyDelegate : ProxyDelegate
        {
            public CollectionProxyDelegate(LinkedList<Func<object, object>> propertyGetters, LinkedList<ProxyDelegate> children)
                : base(propertyGetters, children)
            {
            }

            public override void Execute(object obj, LinkedList<TSearch> resultList, bool stepIntoFound, HashSet<object> visited = null)
            {
                HandleFinalPropertyValues(obj, PropertyGetters.First, resultList, stepIntoFound, visited);
            }
        }
    }
}