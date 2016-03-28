namespace LinqToDB.SqlQuery.Search
{
    using System;

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

        public void Find<TElement>(TBaseSearchInterface source)
        {
            Find(source, typeof(TElement));
        }

        public void Find(TBaseSearchInterface source, Type searchType)
        {
            _pathBuilder.Find(source, searchType);
        }
    }
}