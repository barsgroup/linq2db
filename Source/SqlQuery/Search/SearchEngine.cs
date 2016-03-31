namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.Search.PathBuilder;
    using LinqToDB.SqlQuery.Search.TypeGraph;

    public class SearchEngine<TBaseSearchInterface>
    {
        private readonly PathBuilder<TBaseSearchInterface> _pathBuilder;
        private readonly Dictionary<Tuple<Type, Type>, Delegate> _delegateCache = new Dictionary<Tuple<Type, Type>, Delegate>();

        private SearchEngine()
        {
            var types = typeof(TypeVertex).Assembly.GetTypes();
            var typeGraph = new TypeGraph<TBaseSearchInterface>(types);
            _pathBuilder = new PathBuilder<TBaseSearchInterface>(typeGraph);
        }

        public static SearchEngine<TBaseSearchInterface> Current { get; } = new SearchEngine<TBaseSearchInterface>();

        public LinkedList<TElement> Find<TElement>(TBaseSearchInterface source, bool stepIntoFound) where TElement : class, TBaseSearchInterface
        {
            var result = new LinkedList<TElement>();
            
            var deleg = GetOrCreateDelegate<TElement>(source);
            deleg.Invoke(source, result, stepIntoFound);

# if DEBUG
            var isEqualToReflection = ReflectionSearcher.FindAndCompare(source, stepIntoFound, result);

            if (!isEqualToReflection)
            {
                throw new Exception("result not corresponding to reflection");
            }
#endif

            return result;
        }

        public ResultDelegate<TElement> GetOrCreateDelegate<TElement>(TBaseSearchInterface source) where TElement : class
        {
            var key = Tuple.Create(source.GetType(), typeof(TElement));
            Delegate cachedDelegate;
            if (!_delegateCache.TryGetValue(key, out cachedDelegate))
            {
                var paths = _pathBuilder.Find<TElement>(source);

                var delegateConstructor = new DelegateConstructor<TElement>();
                cachedDelegate = delegateConstructor.CreateResultDelegate(paths);
                _delegateCache[key] = cachedDelegate;
            }

            return (ResultDelegate<TElement>)cachedDelegate;
        }
    }
}