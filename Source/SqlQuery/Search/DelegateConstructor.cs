namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    using System.Text;

    using DynamicExpresso;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search.PathBuilder;
    using LinqToDB.SqlQuery.Search.Utils;

    using Seterlund.CodeGuard;

    public class DelegateConstructor<TSearch>
        where TSearch : class
    {
        public delegate void ResultDelegate(object obj, LinkedList<TSearch> resultList);

        public ResultDelegate CreateResultDelegate(LinkedList<CompositPropertyVertex> vertices)
        {
            var delegateMap = new Dictionary<CompositPropertyVertex, ProxyDelegate>();

            CreateDelegate(vertices.First.Value, delegateMap);

            return delegateMap[vertices.First.Value].Delegate;
        }


        static string GetFullName(Type t)
        {
            if (!t.IsGenericType)
                return t.Name;

            var sb = new StringBuilder();

            sb.Append(t.Name.Substring(0, t.Name.LastIndexOf("`", StringComparison.Ordinal)));
            sb.Append(t.GetGenericArguments().Aggregate("<",
                (aggregate, type) => aggregate + (aggregate == "<"
                                                      ? ""
                                                      : ",") + GetFullName(type)));
            sb.Append(">");

            return sb.ToString();
        }

        private void CreateDelegate(CompositPropertyVertex vertex,
                                    Dictionary<CompositPropertyVertex, ProxyDelegate> delegateMap)
        {
            Guard.That(vertex.PropertyList.First).IsNotNull();

            if (delegateMap.ContainsKey(vertex))
            {
                return;
            }

            var propertyGetters = new LinkedList<Func<object, object>>();

            var propertyGetterInterpretter = new Interpreter();
            const string PropertyAccessExpr = "obj is {0} ? (({0})obj).{1} : null";
            vertex.PropertyList.ForEach(
                node =>
                {
                    propertyGetterInterpretter = propertyGetterInterpretter.Reference(node.Value.DeclaringType);
                    var propGet = string.Format(PropertyAccessExpr, GetFullName(node.Value.DeclaringType), node.Value.Name);

                    var deleg = propertyGetterInterpretter.ParseAsDelegate<Func<object, object>>(propGet, "obj");
                    propertyGetters.AddLast(deleg);
                });

            ResultDelegate findDelegate = (obj, resultList) => 
            {
                if (obj == null)
                {
                    return;
                }

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
                        var searchObj = node.Value as TSearch;
                        if (searchObj != null)
                        {
                            resultList.AddLast(searchObj);
                        }

                        vertex.Children.ForEach(
                            childNode =>
                            {
                                delegateMap[childNode.Value].Delegate(node.Value, resultList);
                            });
                    });
            };

            delegateMap[vertex] = new ProxyDelegate {Delegate = findDelegate};

            vertex.Children.ForEach(
                node =>
                {
                    CreateDelegate(node.Value, delegateMap);
                });

        }

        class ProxyDelegate
        {
            public ResultDelegate Delegate { get; set; } 
        }
    }
}