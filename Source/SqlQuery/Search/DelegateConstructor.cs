namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search.PathBuilder;
    using LinqToDB.SqlQuery.Search.Utils;

    public delegate void ResultDelegate<TSearch>(object obj, LinkedList<TSearch> resultList, bool stepIntoFound, HashSet<object> visited = null);

    public delegate void GetterDelegate(object obj, LinkedListNode<Func<object, object>> getterNode, LinkedList<object> resultList);

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
                                node.Value.Delegate.Invoke(obj, resultList, stepIntoFound, visited);
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
            ResultDelegate<TSearch> findDelegate;

            if (!hasCollection)
            {
                findDelegate = (obj, resultList, stepIntoFound, visited) =>
                    {
                        var currentObj = obj;
                        var curDelegateNode = propertyGetters.First;

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

                        HandleValue(currentObj, resultList, stepIntoFound, visited, childDelegates);
                    };
            }
            else
            {
                findDelegate = (obj, resultList, stepIntoFound, visited) =>
                    {
                        ApplyToFinalPropertyValues(obj, propertyGetters.First, value => HandleValue(value, resultList, stepIntoFound, visited, childDelegates));
                    };
            }

            delegateMap[vertex] = new ProxyDelegate
                                      {
                                          Delegate = findDelegate
                                      };

            vertex.Children.ForEach(
                node =>
                    {
                        CreateDelegate(node.Value, delegateMap);
                        childDelegates.AddLast(delegateMap[node.Value]);
                    });
        }

        private static void HandleValue(object value, LinkedList<TSearch> resultList, bool stepIntoFound, HashSet<object> visited, LinkedList<ProxyDelegate> nextDelegates)
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

            nextDelegates.ForEach(
                childNode =>
                {
                    childNode.Value.Delegate(value, resultList, stepIntoFound, visited);
                });
        }

        private static void ApplyToFinalPropertyValues(object source, LinkedListNode<Func<object, object>> propertyGetterNode, Action<object> action)
        {
            if (propertyGetterNode == null)
            {
                action(source);
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

                    ApplyToFinalPropertyValues(colItem, nextGetterNode, action);
                }
            }
            else
            {
                ApplyToFinalPropertyValues(nextObj, nextGetterNode, action);
            }
        }

        class ProxyDelegate
        {
            public ResultDelegate<TSearch> Delegate { get; set; }
        }

        class ProxyGetter
        {
            public GetterDelegate Getter { get; set; }
        }
    }
}