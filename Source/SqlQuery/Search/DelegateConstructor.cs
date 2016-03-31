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

            vertices.ForEach(
                node =>
                    {
                        CreateDelegate(node.Value, delegateMap);
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
                    if (searchObj != null)
                    {
                        resultList.AddLast(searchObj);

                        if (!stepIntoFound)
                        {
                            return;
                        }
                    }

                    vertices.ForEach(
                        node =>
                            {
                                delegateMap[node.Value].Delegate.Invoke(obj, resultList, stepIntoFound, visited);
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

            vertex.PropertyList.ForEach(
                node =>
                    {
                        var cast = Expression.Convert(parameter, node.Value.DeclaringType);
                        var memberAccess = Expression.Convert(Expression.Property(cast, node.Value.Name), typeof(object));
                        var checkType = Expression.TypeIs(parameter, node.Value.DeclaringType);
                        var conditionalMemberAccess = Expression.Condition(checkType, memberAccess, nullConst);

                        var deleg = Expression.Lambda<Func<object, object>>(conditionalMemberAccess, parameter).Compile();

                        propertyGetters.AddLast(deleg);
                    });

            ResultDelegate<TSearch> findDelegate = (obj, resultList, stepIntoFound, visited) =>
                {
                    LinkedList<object> currentObjects = new LinkedList<object>(new[] { obj });
                    LinkedList<object> nextObjects = new LinkedList<object>();
                    var curDelegateNode = propertyGetters.First;

                    do
                    {
                        var delegateNode = curDelegateNode;

                        currentObjects.ForEach(
                            currentNode =>
                                {
                                    var nextObj = delegateNode.Value(currentNode.Value);

                                    if (nextObj == null)
                                    {
                                        currentObjects.Remove(currentNode);
                                        return;
                                    }

                                    if (CollectionUtils.IsCollection(nextObj.GetType()))
                                    {
                                        var colItems = CollectionUtils.GetCollectionItem(nextObj);
                                        nextObjects.AddRange(colItems);
                                    }
                                    else
                                    {
                                        nextObjects.AddLast(nextObj);
                                    }
                                });

                        currentObjects.Clear();
                        currentObjects.AddRange(nextObjects);
                        nextObjects.Clear();

                        curDelegateNode = curDelegateNode.Next;
                    }
                    while (currentObjects.First != null && curDelegateNode != null);

                    currentObjects.ForEach(
                        node =>
                            {
                                var value = node.Value;

                                if (value == null || visited.Contains(value))
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

                                vertex.Children.ForEach(
                                    childNode =>
                                        {
                                            delegateMap[childNode.Value].Delegate(value, resultList, stepIntoFound, visited);
                                        });
                            });
                };

            delegateMap[vertex] = new ProxyDelegate
                                      {
                                          Delegate = findDelegate
                                      };

            vertex.Children.ForEach(
                node =>
                    {
                        CreateDelegate(node.Value, delegateMap);
                    });
        }

        class ProxyDelegate
        {
            public ResultDelegate<TSearch> Delegate { get; set; }
        }
    }
}