namespace LinqToDB.SqlQuery.Search.PathBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using LinqToDB.SqlQuery.Search.TypeGraph;

    public class PathBuilderSearchCache
    {
        public Type SourceType { get; }

        public Type SearchType { get; }

        public Dictionary<TypeVertex, HashSet<PropertyInfoVertex>> AllVertices { get; } = new Dictionary<TypeVertex, HashSet<PropertyInfoVertex>>();

        public Dictionary<PropertyInfo, PropertyInfoVertex> AllProperties { get; } = new Dictionary<PropertyInfo, PropertyInfoVertex>();
        public Dictionary<PropertyInfo, CompositPropertyVertex> AllCompositProperties { get; } = new Dictionary<PropertyInfo, CompositPropertyVertex>();
        
        public LinkedList<CompositPropertyVertex> OptimizedPaths { get; set; }
        
        public PathBuilderSearchCache(Type sourceType, Type searchType)
        {
            SourceType = sourceType;
            SearchType = searchType;
        }
    }
}