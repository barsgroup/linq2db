namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using System.Text;

    using DynamicExpresso;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search.PathBuilder;

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

            var propertyGetters = new List<Func<object, object>>(vertex.PropertyList.Count);

            var propertyGetterInterpretter = new Interpreter();
            const string PropertyAccessExpr = "obj is {0} ? (({0})obj).{1} : null";
            vertex.PropertyList.ForEach(
                node =>
                {
                    propertyGetterInterpretter = propertyGetterInterpretter.Reference(node.Value.DeclaringType);
                    var propGet = string.Format(PropertyAccessExpr, GetFullName(node.Value.DeclaringType), node.Value.Name);

                    var deleg = propertyGetterInterpretter.ParseAsDelegate<Func<object, object>>(propGet, "obj");
                    propertyGetters.Add(deleg);
                });

            ResultDelegate findDelegate = (obj, resultList) => 
            {
                var currentObj = obj;
                for (var i = 0; i < propertyGetters.Count; i++)
                {
                    currentObj = propertyGetters[i](currentObj);

                    if (currentObj == null)
                    {
                        return;
                    }
                }

                var searchObj = currentObj as TSearch;
                if (searchObj != null)
                {
                    resultList.AddLast(searchObj);
                }

                vertex.Children.ForEach(
                    node =>
                    {
                        delegateMap[node.Value].Delegate(obj, resultList);
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