namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.Search.PathBuilder;
    using LinqToDB.SqlQuery.Search.TypeGraph;

    public class SearchEngine<TBaseSearchInterface>
    {
        private readonly PathBuilder<TBaseSearchInterface> _pathBuilder;
        private readonly Dictionary<TypeKey, Delegate> _delegateCache = new Dictionary<TypeKey, Delegate>();

        private SearchEngine()
        {
            var types = typeof(TypeVertex).Assembly.GetTypes();
            var typeGraph = new TypeGraph<TBaseSearchInterface>(types);
            _pathBuilder = new PathBuilder<TBaseSearchInterface>(typeGraph);
        }

        public static SearchEngine<TBaseSearchInterface> Current { get; } = new SearchEngine<TBaseSearchInterface>();

        public void Find<TElement>(TBaseSearchInterface source, LinkedList<TElement> resultList, FindStrategy<TElement> strategy, HashSet<object> visited) where TElement : class, TBaseSearchInterface
        {
            Find(source, resultList, strategy, visited, null);
        }

        public void Find<TElement, TResult>(
            TBaseSearchInterface source, 
            LinkedList<TResult> resultList,
            SearchStrategy<TElement, TResult> strategy,
            HashSet<object> visited,
            Func<TElement, TResult> action) where TElement : class, TBaseSearchInterface
        {
            var deleg = GetOrCreateDelegate(source, strategy);
            deleg.Invoke(source, resultList, visited, action);
        }

        public ResultDelegate<TElement, TResult> GetOrCreateDelegate<TElement, TResult>(TBaseSearchInterface source, SearchStrategy<TElement, TResult> strategy) where TElement : class
        {
            var key = new TypeKey(strategy.GetType(), source.GetType(), typeof(TElement));
            Delegate cachedDelegate;
            if (!_delegateCache.TryGetValue(key, out cachedDelegate))
            {
                var paths = _pathBuilder.Find<TElement>(source);

                var delegateConstructor = new DelegateConstructor<TElement, TResult>();
                cachedDelegate = delegateConstructor.CreateResultDelegate(paths, strategy);
                _delegateCache[key] = cachedDelegate;
            }

            return (ResultDelegate<TElement, TResult>)cachedDelegate;
        }
    }
}