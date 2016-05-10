using Bars2Db.SqlQuery.Search.PathBuilder;
using Bars2Db.SqlQuery.Search.TypeGraph;
using LinqToDB.Tests.SearchEngine.PathBuilder.Find.Base;
using LinqToDB.Tests.SearchEngine.TestInterfaces.CastFromBase;
using Xunit;

namespace LinqToDB.Tests.SearchEngine.PathBuilder.Find
{
    public class CastFromBaseTest : BaseFindTest
    {
        [Fact]
        public void Test()
        {
            var typeGraph = new TypeGraph<IBase>(GetType().Assembly.GetTypes());

            var pathBuilder = new PathBuilder<IBase>(typeGraph);

            var result = pathBuilder.Find(new A(), typeof(ID));

            var ab = typeof(IA).GetProperty("Base");
            var bc = typeof(IB).GetProperty("Base");
            var cd = typeof(IC).GetProperty("Base");

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

            var expected = new[] {vertexAB};

            Assert.True(IsEqual(expected, result));
        }
    }
}