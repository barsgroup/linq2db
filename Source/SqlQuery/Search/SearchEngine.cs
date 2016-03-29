namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.Search.PathBuilder;
    using LinqToDB.SqlQuery.Search.TypeGraph;

    public class SearchEngine<TBaseSearchInterface>
    {
        private readonly PathBuilder<TBaseSearchInterface> _pathBuilder;

        private SearchEngine()
        {
            var types = typeof(TypeVertex).Assembly.GetTypes();
            var typeGraph = new TypeGraph<TBaseSearchInterface>(types);
            _pathBuilder = new PathBuilder<TBaseSearchInterface>(typeGraph);
        }

        public static SearchEngine<TBaseSearchInterface> Current { get; } = new SearchEngine<TBaseSearchInterface>();

        public LinkedList<TElement> Find<TElement>(TBaseSearchInterface source) where TElement : class
        {
            var paths = _pathBuilder.Find<TElement>(source);
            
            var delegateConstructor = new DelegateConstructor<TElement>();
            var deleg = delegateConstructor.CreateResultDelegate(paths);

            var result = new LinkedList<TElement>();
            deleg.Invoke(source, result);

            return result;
        }
    }
}