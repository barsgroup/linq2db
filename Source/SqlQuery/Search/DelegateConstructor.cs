namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search.PathBuilder;
    using LinqToDB.SqlQuery.Search.Utils;

    public delegate void ResultDelegate<TSearch>(object obj, LinkedList<TSearch> resultList, StrategyDelegate<TSearch> strategyDelegate, HashSet<object> visited) where TSearch : class;

    public delegate void StrategyDelegate<TSearch>(BaseProxyDelegate<TSearch> current, object obj, LinkedList<TSearch> resultList, HashSet<object> visited) where TSearch : class;

    public class DelegateConstructor<TSearch>
        where TSearch : class
    {
        public ResultDelegate<TSearch> CreateResultDelegate(LinkedList<CompositPropertyVertex> vertices)
        {
            var delegateMap = new Dictionary<CompositPropertyVertex, ProxyDelegate<TSearch>>();

            var delegates = new ProxyDelegate<TSearch>[vertices.Count];

            var index = 0;
            vertices.ForEach(
                node =>
                    {
                        CreateDelegate(node.Value, delegateMap);
                        delegates[index++] = delegateMap[node.Value];
                    });

            var rootDelegate = new RootProxyDelegate<TSearch>(delegates);
            rootDelegate.BuildExecuteChildrenDelegate();

            return rootDelegate.Execute;
        }

        private static void CreateDelegate(CompositPropertyVertex vertex, Dictionary<CompositPropertyVertex, ProxyDelegate<TSearch>> delegateMap)
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

            var childDelegates = new ProxyDelegate<TSearch>[vertex.Children.Count];

            delegateMap[vertex] = new ProxyDelegate<TSearch>(propertyGetters, childDelegates, hasCollection);

            index = 0;
            vertex.Children.ForEach(
                node =>
                    {
                        CreateDelegate(node.Value, delegateMap);
                        childDelegates[index++] = delegateMap[node.Value];
                    });
        }
    }

    public sealed class ProxyDelegate<TSearch> : BaseProxyDelegate<TSearch>
        where TSearch : class
    {
        private readonly Func<object, object>[] _propertyGetters;

        private readonly bool _isCollection;

        public ProxyDelegate(Func<object, object>[] propertyGetters, ProxyDelegate<TSearch>[] children, bool isCollection): base(children)
        {
            _isCollection = isCollection;

            _propertyGetters = propertyGetters;
        }

        public void Execute(object obj, LinkedList<TSearch> resultList, StrategyDelegate<TSearch> strategyDelegate, HashSet<object> visited = null)
        {
            if (_isCollection)
            {
                CollectionExecute(obj, resultList, strategyDelegate, visited);
            }
            else
            {
                ScalarExecute(obj, resultList, strategyDelegate, visited);
            }
        }


        private void HandleFinalPropertyValues(object source, int index, LinkedList<TSearch> resultList, StrategyDelegate<TSearch> strategyDelegate, HashSet<object> visited)
        {
            while (true)
            {
                if (index == _propertyGetters.Length)
                {
                    HandleValue(this, source, resultList, strategyDelegate, visited);
                    return;
                }

                var nextObj = _propertyGetters[index](source);

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
                        if (colItems[i] != null)
                        {
                            HandleFinalPropertyValues(colItems[i], nextIndex, resultList, strategyDelegate, visited);
                        }
                    }
                    break;
                }

                source = nextObj;
                index = nextIndex;

            }
        }

        private void ScalarExecute(object obj, LinkedList<TSearch> resultList, StrategyDelegate<TSearch> strategyDelegate, HashSet<object> visited)
        {
            var currentObj = obj;

            for (var i = 0; i < _propertyGetters.Length; ++i)
            {
                currentObj = _propertyGetters[i](currentObj);

                if (currentObj == null)
                {
                    return;
                }
            }

            HandleValue(this, currentObj, resultList, strategyDelegate, visited);
        }

        private void CollectionExecute(object obj, LinkedList<TSearch> resultList, StrategyDelegate<TSearch> strategyDelegate, HashSet<object> visited)
        {
            HandleFinalPropertyValues(obj, 0, resultList, strategyDelegate, visited);
        }
    }

    internal sealed class RootProxyDelegate<TSearch> : BaseProxyDelegate<TSearch>
        where TSearch : class
    {
        public RootProxyDelegate(ProxyDelegate<TSearch>[] children): base(children)
        {
            IsRoot = true;
        }

        public void Execute(object obj, LinkedList<TSearch> resultList, StrategyDelegate<TSearch> strategyDelegate, HashSet<object> visited)
        {
            if (obj == null)
            {
                return;
            }

            HandleValue(this, obj, resultList, strategyDelegate,  visited);
        }
    }

    public abstract class BaseProxyDelegate<TSearch>
        where TSearch : class
    {
        public readonly ProxyDelegate<TSearch>[] Children;

        public ResultDelegate<TSearch> ExecuteChildrenDelegate; 

        public bool IsRoot = false;

        protected static void HandleValue(BaseProxyDelegate<TSearch> current, object value, LinkedList<TSearch> resultList, StrategyDelegate<TSearch> strategyDelegate, HashSet<object> visited)
        {
            if (visited.Contains(value))
            {
                return;
            }

            visited.Add(value);

            strategyDelegate(current, value, resultList, visited);
        }

        public void BuildExecuteChildrenDelegate()
        {
            if (ExecuteChildrenDelegate != null)
            {
                return;
            }

            if (Children.Length == 0)
            {
                ExecuteChildrenDelegate = (obj, resultList, strategy, visited) => {};

                return;
            }

            var executeMethodInfo = typeof(ProxyDelegate<TSearch>).GetMethod("Execute");

            var objParam = Expression.Parameter(typeof(object), "obj");
            var resultListParam = Expression.Parameter(typeof(LinkedList<TSearch>), "resultList");
            var strategyParam = Expression.Parameter(typeof(StrategyDelegate<TSearch>), "strategy");
            var visitedParam = Expression.Parameter(typeof(HashSet<object>), "visited");

            var paramArray = new[] { objParam, resultListParam, strategyParam, visitedParam };

            var callChildrenExpressions = Children.Select(Expression.Constant).Select(childExpr => Expression.Call(childExpr, executeMethodInfo, paramArray));

            var callChildrenBlock = Expression.Block(callChildrenExpressions);

            var lambda = Expression.Lambda<ResultDelegate<TSearch>>(callChildrenBlock, paramArray);

            ExecuteChildrenDelegate = lambda.Compile();

            for (var i = 0; i < Children.Length; ++i)
            {
                Children[i].BuildExecuteChildrenDelegate();
            }
        }

        protected BaseProxyDelegate(ProxyDelegate<TSearch>[] children)
        {
            Children = children;
        }
    }
}