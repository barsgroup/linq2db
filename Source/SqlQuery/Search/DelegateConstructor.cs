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

    public delegate void ResultDelegate<TSearch>(object obj, LinkedList<TSearch> resultList, HashSet<object> visited) where TSearch : class;

    public delegate void StrategyDelegate<TSearch>(BaseProxyDelegate<TSearch> current, object obj, LinkedList<TSearch> resultList, HashSet<object> visited) where TSearch : class;

    public class DelegateConstructor<TSearch>
        where TSearch : class
    {
        public ResultDelegate<TSearch> CreateResultDelegate(LinkedList<CompositPropertyVertex> vertices, SearchStrategy<TSearch> strategy)
        {
            var delegateMap = new Dictionary<CompositPropertyVertex, ProxyDelegate<TSearch>>();

            var delegates = new ProxyDelegate<TSearch>[vertices.Count];

            var index = 0;
            vertices.ForEach(
                node =>
                    {
                        CreateDelegate(node.Value, delegateMap, strategy);
                        delegates[index++] = delegateMap[node.Value];
                    });

            var rootDelegate = new RootProxyDelegate<TSearch>(delegates, strategy);
            rootDelegate.BuildExecuteChildrenDelegate();

            return rootDelegate.Execute;
        }

        private static void CreateDelegate(CompositPropertyVertex vertex, Dictionary<CompositPropertyVertex, ProxyDelegate<TSearch>> delegateMap, SearchStrategy<TSearch> strategy)
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

                        var resultVariable = Expression.Variable(typeof(object), "result");
                        
                        var castVariable = Expression.Variable(node.Value.DeclaringType, "value");
                        var castAs = Expression.TypeAs(parameter, node.Value.DeclaringType);
                        var castAssign = Expression.Assign(castVariable, castAs);
                        
                        var checkNotNull = Expression.NotEqual(castVariable, nullConst);
                        var memberAccess = Expression.Convert(Expression.MakeMemberAccess(castVariable, node.Value), typeof(object));
                        var conditionalMemberAccess = Expression.Condition(checkNotNull, memberAccess, nullConst);
                        
                        var resultAssign = Expression.Assign(resultVariable, conditionalMemberAccess);
                        
                        var returnTarget = Expression.Label(typeof(object));
                        var returnLabel = Expression.Label(returnTarget, nullConst);
                        var returnExpr = Expression.Return(returnTarget, resultVariable);
                        
                        var block = Expression.Block(typeof(object), new[] { castVariable, resultVariable }, castAssign, resultAssign, returnExpr, returnLabel);
                        
                        var deleg = Expression.Lambda<Func<object, object>>(block, parameter).Compile();


                        ////var castAs = Expression.TypeAs(parameter, node.Value.DeclaringType);
                        ////var checkNotNull = Expression.NotEqual(castAs, nullConst);
                        ////var memberAccess = Expression.Convert(Expression.MakeMemberAccess(castAs, node.Value), typeof(object));
                        ////var conditionalMemberAccess = Expression.Condition(checkNotNull, memberAccess, nullConst);
                        ////
                        ////var deleg = Expression.Lambda<Func<object, object>>(conditionalMemberAccess, parameter).Compile();

                        propertyGetters[index++] = deleg;
                    });

            var childDelegates = new ProxyDelegate<TSearch>[vertex.Children.Count];

            delegateMap[vertex] = new ProxyDelegate<TSearch>(propertyGetters, childDelegates, strategy, hasCollection);

            index = 0;
            vertex.Children.ForEach(
                node =>
                    {
                        CreateDelegate(node.Value, delegateMap, strategy);
                        childDelegates[index++] = delegateMap[node.Value];
                    });
        }
    }

    public sealed class ProxyDelegate<TSearch> : BaseProxyDelegate<TSearch>
        where TSearch : class
    {
        private readonly Func<object, object>[] _propertyGetters;

        private readonly bool _isCollection;

        public ProxyDelegate(Func<object, object>[] propertyGetters, ProxyDelegate<TSearch>[] children, SearchStrategy<TSearch> strategy, bool isCollection): base(children, strategy)
        {
            _isCollection = isCollection;

            _propertyGetters = propertyGetters;
        }

        public void Execute(object obj, LinkedList<TSearch> resultList, HashSet<object> visited = null)
        {
            if (_isCollection)
            {
                CollectionExecute(obj, resultList, visited);
            }
            else
            {
                ScalarExecute(obj, resultList, visited);
            }
        }


        private void HandleFinalPropertyValues(object source, int index, LinkedList<TSearch> resultList, HashSet<object> visited)
        {
            while (true)
            {
                if (index == _propertyGetters.Length)
                {
                    HandleValue(this, source, resultList, visited);
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
                            HandleFinalPropertyValues(colItems[i], nextIndex, resultList, visited);
                        }
                    }
                    break;
                }

                source = nextObj;
                index = nextIndex;

            }
        }

        private void ScalarExecute(object obj, LinkedList<TSearch> resultList, HashSet<object> visited)
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

            HandleValue(this, currentObj, resultList, visited);
        }

        private void CollectionExecute(object obj, LinkedList<TSearch> resultList, HashSet<object> visited)
        {
            HandleFinalPropertyValues(obj, 0, resultList, visited);
        }
    }

    internal sealed class RootProxyDelegate<TSearch> : BaseProxyDelegate<TSearch>
        where TSearch : class
    {
        public RootProxyDelegate(ProxyDelegate<TSearch>[] children, SearchStrategy<TSearch> strategy) : base(children, strategy)
        {
            IsRoot = true;
        }

        public void Execute(object obj, LinkedList<TSearch> resultList, HashSet<object> visited)
        {
            if (obj == null)
            {
                return;
            }

            HandleValue(this, obj, resultList, visited);
        }
    }

    public abstract class BaseProxyDelegate<TSearch>
        where TSearch : class
    {
        public readonly ProxyDelegate<TSearch>[] Children;

        private StrategyDelegate<TSearch> _strategyDelegate;
        private SearchStrategy<TSearch> _strategy;

        public bool IsRoot = false;

        protected static void HandleValue(BaseProxyDelegate<TSearch> current, object value, LinkedList<TSearch> resultList, HashSet<object> visited)
        {
            if (visited.Contains(value))
            {
                return;
            }

            visited.Add(value);

            current._strategyDelegate(current, value, resultList, visited);
        }

        public void BuildExecuteChildrenDelegate()
        {
            if (_strategyDelegate != null)
            {
                return;
            }
            
            var paramArray = new[] { _strategy.ObjParam, _strategy.ResultListParam, _strategy.VisitedParam };

            Expression executeChildrenExpression;

            if (Children.Length == 0)
            {
                executeChildrenExpression = Expression.Empty();
            }
            else
            {
                var executeMethodInfo = typeof(ProxyDelegate<TSearch>).GetMethod("Execute");

                var callChildrenExpressions = Children.Select(Expression.Constant).Select(childExpr => Expression.Call(childExpr, executeMethodInfo, paramArray));
                
                executeChildrenExpression = Expression.Block(callChildrenExpressions);
            }

            _strategyDelegate = _strategy.GetStrategyExpression(executeChildrenExpression, IsRoot).Compile();

            for (var i = 0; i < Children.Length; ++i)
            {
                Children[i].BuildExecuteChildrenDelegate();
            }
        }

        protected BaseProxyDelegate(ProxyDelegate<TSearch>[] children, SearchStrategy<TSearch>  strategy)
        {
            Children = children;
            _strategy = strategy;
        }
    }
}