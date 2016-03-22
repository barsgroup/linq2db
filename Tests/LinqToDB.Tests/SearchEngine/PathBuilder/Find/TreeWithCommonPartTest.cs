namespace LinqToDB.Tests.SearchEngine.PathBuilder.Find
{
    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;
    using LinqToDB.Tests.SearchEngine.PathBuilder.Find.Base;

    using Xunit;

    public class TreeWithCommonPartTest : BaseFindTest
    {
        public interface IBase
        {
            [SearchContainer]
            IBase Base { get; set; }
        }

        public interface IA : IBase
        {
            [SearchContainer]
            IB B { get; set; }

            [SearchContainer]
            IBase X { get; set; }
        }

        public interface IB : IBase
        {
            [SearchContainer]
            IC C { get; set; }

            [SearchContainer]
            IBase X { get; set; }
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
            IBase X { get; set; }
        }

        public interface ID : IBase
        {
        }

        public class A : IA
        {
            public IB B { get; set; }

            public IBase X { get; set; }

            public IC C { get; set; }

            public IBase Base { get; set; }
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
            startVertex.Children.AddRange(new[] { cd1Vertex, cd2Vertex, cd3Vertex });

            var expected = new[] { startVertex };

            Assert.True(IsEqual(expected, result));
        }
    }
}
