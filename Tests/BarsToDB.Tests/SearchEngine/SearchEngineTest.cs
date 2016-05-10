//namespace LinqToDB.Tests.SearchEngine
//{
//    using LinqToDB.SqlQuery.QueryElements;
//    using LinqToDB.SqlQuery.QueryElements.Interfaces;
//    using LinqToDB.SqlQuery.Search;

//    using Xunit;

//    public class SearchEngineTest
//    {
//        [Fact]
//        public void DelegateCacheTest()
//        {
//            var engine = SearchEngine<IQueryElement>.Current;

//            var selectQuery = new SelectQuery();

//            var deleg1 = engine.GetOrCreateDelegate<IColumn>(selectQuery);
//            var deleg2 = engine.GetOrCreateDelegate<IColumn>(selectQuery);

//            Assert.True(ReferenceEquals(deleg1, deleg2));
//        }
//    }
//}

