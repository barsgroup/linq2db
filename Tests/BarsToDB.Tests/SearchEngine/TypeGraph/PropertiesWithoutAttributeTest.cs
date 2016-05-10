namespace LinqToDB.Tests.SearchEngine.TypeGraphEx
{
    using LinqToDB.SqlQuery.Search;
    using LinqToDB.SqlQuery.Search.TypeGraph;
    using LinqToDB.Tests.SearchEngine.TypeGraph.Base;

    using Xunit;

    public class PropertiesWithoutAttributeTest : TypeGraphBaseTest
    {
        public interface IBase
        {
            [SearchContainer]
            IA A1 { get; set; }
            
            IA A2 { get; set; }
        }

        public interface IA : IBase
        {
        }

        [Fact]
        public void Test()
        {
            var counter = 0;
            var baseVertex = new TypeVertex(typeof(IBase), counter++);
            var a = new TypeVertex(typeof(IA), counter++);

            var propBaseA1 = typeof(IBase).GetProperty("A1");
            var basea = new PropertyEdge(baseVertex, propBaseA1, a);

            var expectedGraph = GetGraphArray(baseVertex, a);

            //// IBase -> [{IBase.A1, IA}]
            ////       ~> [IA]
            //// 
            //// IA    -> []
            ////       ~> [IBase]

            expectedGraph[baseVertex.Index].Children.AddLast(basea);
            expectedGraph[baseVertex.Index].Casts.AddLast(new CastEdge(baseVertex, a));

            expectedGraph[a.Index].Parents.AddLast(basea);
            expectedGraph[a.Index].Casts.AddLast(new CastEdge(a, baseVertex));

            var typeGraph = BuildTypeGraph<IBase>();

            Assert.True(IsEqual(expectedGraph, typeGraph.Vertices));
        }
    }
}
