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

    public delegate void ResultDelegate<TSearch, TResult>(object obj, LinkedList<TResult> resultList, HashSet<object> visited, Func<TSearch, TResult> func) where TSearch : class;

    public class DelegateConstructor<TSearch, TResult>
        where TSearch : class
    {
        public ResultDelegate<TSearch, TResult> CreateResultDelegate(LinkedList<CompositPropertyVertex> vertices, SearchStrategy<TSearch, TResult> strategy)
        {
            var delegateMap = new Dictionary<CompositPropertyVertex, ProxyDelegate<TSearch, TResult>>();

            var delegates = new ProxyDelegate<TSearch, TResult>[vertices.Count];

            var index = 0;
            vertices.ForEach(
                node =>
                    {
                        CreateDelegate(node.Value, delegateMap, strategy);
                        delegates[index++] = delegateMap[node.Value];
                    });

            var rootDelegate = new RootProxyDelegate<TSearch, TResult>(delegates, strategy);
            rootDelegate.BuildExecuteChildrenDelegate();

            return rootDelegate.Execute;
        }

        private static void CreateDelegate(CompositPropertyVertex vertex, Dictionary<CompositPropertyVertex, ProxyDelegate<TSearch, TResult>> delegateMap, SearchStrategy<TSearch, TResult> strategy)
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

                        propertyGetters[index++] = deleg;
                    });

            var childDelegates = new ProxyDelegate<TSearch, TResult>[vertex.Children.Count];

            delegateMap[vertex] = new ProxyDelegate<TSearch, TResult>(propertyGetters, childDelegates, strategy, hasCollection);

            index = 0;
            vertex.Children.ForEach(
                node =>
                    {
                        CreateDelegate(node.Value, delegateMap, strategy);
                        childDelegates[index++] = delegateMap[node.Value];
                    });
        }
    }

    public sealed class ProxyDelegate<TSearch, TResult> : BaseProxyDelegate<TSearch, TResult>
        where TSearch : class
    {
        private readonly Func<object, object>[] _propertyGetters;

        private readonly bool _isCollection;

        public ProxyDelegate(Func<object, object>[] propertyGetters, ProxyDelegate<TSearch, TResult>[] children, SearchStrategy<TSearch, TResult> strategy, bool isCollection): base(children, strategy)
        {
            _isCollection = isCollection;

            _propertyGetters = propertyGetters;
        }

        public void Execute(object obj, LinkedList<TResult> resultList, HashSet<object> visited, Func<TSearch, TResult> func)
        {
            if (_isCollection)
            {
                CollectionExecute(obj, resultList, visited, func);
            }
            else
            {
                ScalarExecute(obj, resultList, visited, func);
            }
        }


        private void HandleFinalPropertyValues(object source, int index, LinkedList<TResult> resultList, HashSet<object> visited, Func<TSearch, TResult> func)
        {
            while (true)
            {
                if (index == _propertyGetters.Length)
                {
                    HandleValue(this, source, resultList, visited, func);
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
                            HandleFinalPropertyValues(colItems[i], nextIndex, resultList, visited, func);
                        }
                    }
                    break;
                }

                source = nextObj;
                index = nextIndex;

            }
        }

        private void ScalarExecute(object obj, LinkedList<TResult> resultList, HashSet<object> visited, Func<TSearch, TResult> func)
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

            HandleValue(this, currentObj, resultList, visited, func);
        }

        private void CollectionExecute(object obj, LinkedList<TResult> resultList, HashSet<object> visited, Func<TSearch, TResult> func)
        {
            HandleFinalPropertyValues(obj, 0, resultList, visited, func);
        }
    }

    internal sealed class RootProxyDelegate<TSearch, TResult> : BaseProxyDelegate<TSearch, TResult>
        where TSearch : class
    {
        public RootProxyDelegate(ProxyDelegate<TSearch, TResult>[] children, SearchStrategy<TSearch, TResult> strategy) : base(children, strategy)
        {
            IsRoot = true;
        }

        public void Execute(object obj, LinkedList<TResult> resultList, HashSet<object> visited, Func<TSearch, TResult> func)
        {
            if (obj == null)
            {
                return;
            }

            HandleValue(this, obj, resultList, visited, func);
        }
    }

    public abstract class BaseProxyDelegate<TSearch, TResult>
        where TSearch : class
    {
        public readonly ProxyDelegate<TSearch, TResult>[] Children;

        private ResultDelegate<TSearch, TResult> _strategyDelegate;
        private SearchStrategy<TSearch, TResult> _strategy;

        public bool IsRoot = false;

        protected static void HandleValue(BaseProxyDelegate<TSearch, TResult> current, object value, LinkedList<TResult> resultList, HashSet<object> visited, Func<TSearch, TResult> func)
        {
            if (visited.Contains(value))
            {
                return;
            }

            visited.Add(value);

            current._strategyDelegate(value, resultList, visited, func);
        }

        public void BuildExecuteChildrenDelegate()
        {
            if (_strategyDelegate != null)
            {
                return;
            }
            
            var executeMethodInfo = typeof(ProxyDelegate<TSearch, TResult>).GetMethod("Execute");

            var callChildrenExpressions = Children.Select(Expression.Constant).Select(childExpr => Expression.Call(childExpr, executeMethodInfo, _strategy.ParamArray)).ToArray();
            
            _strategyDelegate = _strategy.GetStrategyExpression(callChildrenExpressions, IsRoot).Compile();

            for (var i = 0; i < Children.Length; ++i)
            {
                Children[i].BuildExecuteChildrenDelegate();
            }
        }

        protected BaseProxyDelegate(ProxyDelegate<TSearch, TResult>[] children, SearchStrategy<TSearch, TResult>  strategy)
        {
            Children = children;
            _strategy = strategy;
        }
    }
}