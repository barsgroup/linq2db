using System.Collections.Generic;
using Bars2Db.SqlQuery.Search;
using Bars2Db.SqlQuery.Search.PathBuilder;
using Bars2Db.SqlQuery.Search.TypeGraph;
using LinqToDB.Tests.SearchEngine.PathBuilder.Find.Base;
using Xunit;

namespace LinqToDB.Tests.SearchEngine.PathBuilder.Find
{
    public class CollectionTest : BaseFindTest
    {
        public interface IBase
        {
        }

        public interface IA : IBase
        {
            [SearchContainer]
            List<IB> ListB { get; set; }
        }

        public interface IB : IBase
        {
        }

        public class A : IA
        {
            public List<IB> ListB { get; set; }
        }

        [Fact]
        public void Test()
        {
            var typeGraph = new TypeGraph<IBase>(GetType().Assembly.GetTypes());

            var pathBuilder = new PathBuilder<IBase>(typeGraph);

            var result = pathBuilder.Find(new A(), typeof(IB));

            var ab = typeof(IA).GetProperty("ListB");

            var vertex1 = new CompositPropertyVertex();
            vertex1.PropertyList.AddLast(ab);

            var expected = new[] {vertex1};

            Assert.True(IsEqual(expected, result));
        }
    }
}