using Bars2Db.Extensions;
using Bars2Db.SqlQuery.Search;
using Bars2Db.SqlQuery.Search.PathBuilder;
using Bars2Db.SqlQuery.Search.TypeGraph;
using LinqToDB.Tests.SearchEngine.PathBuilder.Find.Base;
using Xunit;

namespace LinqToDB.Tests.SearchEngine.PathBuilder.Find
{
    public class TreeWithCommonPartTest : BaseFindTest
    {
        public interface IBase
        {
        }

        public interface IA : IBase
        {
            [SearchContainer]
            IB B { get; set; }

            [SearchContainer]
            IX X { get; set; }
        }

        public interface IB : IBase
        {
            [SearchContainer]
            IC C { get; set; }

            [SearchContainer]
            IX X { get; set; }
        }

        public interface IC : IBase
        {
            [SearchContainer]
            ID D1 { get; set; }

            [SearchContainer]
            ID D2 { get; set; }

            [SearchContainer]
            ID D3 { get; set; }

            [SearchContainer]
            IX X { get; set; }
        }

        public interface ID : IBase
        {
        }

        public interface IX : IBase
        {
        }

        public class A : IA
        {
            public IC C { get; set; }

            public IX Base { get; set; }
            public IB B { get; set; }

            public IX X { get; set; }
        }

        [Fact]
        public void Test()
        {
            var typeGraph = new TypeGraph<IBase>(GetType().Assembly.GetTypes());

            var pathBuilder = new PathBuilder<IBase>(typeGraph);

            var result = pathBuilder.Find(new A(), typeof(ID));

            var ab = typeof(IA).GetProperty("B");
            var bc = typeof(IB).GetProperty("C");
            var cd1 = typeof(IC).GetProperty("D1");
            var cd2 = typeof(IC).GetProperty("D2");
            var cd3 = typeof(IC).GetProperty("D3");

            var cd1Vertex = new CompositPropertyVertex();
            cd1Vertex.PropertyList.AddLast(cd1);

            var cd2Vertex = new CompositPropertyVertex();
            cd2Vertex.PropertyList.AddLast(cd2);

            var cd3Vertex = new CompositPropertyVertex();
            cd3Vertex.PropertyList.AddLast(cd3);

            var startVertex = new CompositPropertyVertex();
            startVertex.PropertyList.AddLast(ab);
            startVertex.PropertyList.AddLast(bc);
            startVertex.Children.AddRange(new[] {cd1Vertex, cd2Vertex, cd3Vertex});

            var expected = new[] {startVertex};

            Assert.True(IsEqual(expected, result));
        }
    }
}