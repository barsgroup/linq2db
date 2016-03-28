namespace LinqToDB.Tests.SearchEngine.PathBuilder.Find
{
    using LinqToDB.SqlQuery.Search;
    using LinqToDB.SqlQuery.Search.PathBuilder;
    using LinqToDB.SqlQuery.Search.TypeGraph;
    using LinqToDB.Tests.SearchEngine.PathBuilder.Find.Base;

    using Xunit;

    public class CastFromBaseTest : BaseFindTest
    {
        public interface IBase
        {
        }

        public interface IA : IBase
        {
            [SearchContainer]
            IBase B { get; set; }
        }

        public interface IB : IBase
        {
            [SearchContainer]
            IBase C { get; set; }
        }

        public interface IC : IBase
        {
            [SearchContainer]
            IBase D { get; set; }
        }

        public interface ID : IBase
        {
        }

        public class A : IA
        {
            public IBase B { get; set; }
        }

        [Fact]
        public void Test()
        {
            var typeGraph = new TypeGraph<IBase>(GetType().Assembly.GetTypes());

            var pathBuilder = new PathBuilder<IBase>(typeGraph);

            var result = pathBuilder.Find(new A(), typeof(ID));

            var ab = typeof(IA).GetProperty("B");
            var bc = typeof(IB).GetProperty("C");
            var cd = typeof(IC).GetProperty("D");

            var vertexAB = new CompositPropertyVertex();
            vertexAB.PropertyList.AddLast(ab);

            var vertexBC = new CompositPropertyVertex();
            vertexBC.PropertyList.AddLast(bc);

            var vertexCD = new CompositPropertyVertex();
            vertexCD.PropertyList.AddLast(cd);

            vertexAB.Children.AddLast(vertexAB);
            vertexAB.Children.AddLast(vertexBC);
            vertexAB.Children.AddLast(vertexCD);

            vertexBC.Children.AddLast(vertexAB);
            vertexBC.Children.AddLast(vertexBC);
            vertexBC.Children.AddLast(vertexCD);

            vertexCD.Children.AddLast(vertexAB);
            vertexCD.Children.AddLast(vertexBC);
            vertexCD.Children.AddLast(vertexCD);

            var expected = new[] { vertexAB };

            Assert.True(IsEqual(expected, result));
        }
    }
}
