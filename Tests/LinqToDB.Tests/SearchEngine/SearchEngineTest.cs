namespace LinqToDB.Tests.SearchEngine
{
    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    using Xunit;

    public class SearchEngineTest
    {
        [Fact]
        public void SimpleTest()
        {
            var engine = SearchEngine<IQueryElement>.Current;

            var selectQuery = new SelectQuery();

            engine.Find<IColumn>(selectQuery);
        }
    }
}