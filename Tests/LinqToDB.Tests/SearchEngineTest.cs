namespace LinqToDB.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    using Xunit;

    public class SearchEngineTest
    {
        [Fact]
        public void SimpleTest()
        {
            var engine = SearchEngine.Current;

            var selectQuery = new SelectQuery();

            engine.Find<IColumn>(selectQuery);
        }

        [Fact]
        public void FindPaths()
        {
            var engine = new TestSeachEngine();
            var paths = engine.FindPaths<ISelectQuery, IColumn>();

            var resultPaths = new LinkedList<LinkedList<PropertyPath>>();
        }

     
        private class TestSeachEngine : SearchEngine
        {
            public readonly Dictionary<PropertyInfo, PropertyPath> AllVertex = new Dictionary<PropertyInfo, PropertyPath>();

            public LinkedList<PropertyPath> FindPaths<TSource, TTarget>()
            {
                return GetFindTree(SearchVertices[typeof(TSource)], typeof(TTarget), typeof(TSource), AllVertex , new HashSet<PropertyInfo>());
            }
        }
    }
}