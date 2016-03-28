namespace LinqToDB.Tests.SearchEngine.TypeGraphEx
{
    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;
    using LinqToDB.Tests.SearchEngine.TypeGraphEx.Base;

    using Xunit;

    public class TwoPropertiesOfOneTypeTest : TypeGraphExBaseTest
    {
        public interface IBase
        {
            [SearchContainer]
            IA A1 { get; set; }

            [SearchContainer]
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
            var propBaseA2 = typeof(IBase).GetProperty("A2");

            var basea1 = new PropertyEdge(baseVertex, propBaseA1, a);
            var basea2 = new PropertyEdge(baseVertex, propBaseA2, a);

            var expectedGraph = GetGraphArray(baseVertex, a);

            //// IBase -> [{IBase.A1, IA}, {IBase.A2, IA}]
            ////       ~> [IA]
            //// 
            ////    IA -> []
            ////       ~> [IBase]

            expectedGraph[baseVertex.Index].Children.AddRange(new[] { basea1, basea2 });
            expectedGraph[baseVertex.Index].Casts.AddLast(new CastEdge(baseVertex, a));

            expectedGraph[a.Index].Parents.AddRange(new[] { basea1, basea2 });
            expectedGraph[a.Index].Casts.AddLast(new CastEdge(a, baseVertex));

            var typeGraph = BuildTypeGraph<IBase>();

            Assert.True(IsEqual(expectedGraph, typeGraph.Vertices));
        }
    }
}
