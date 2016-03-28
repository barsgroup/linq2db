namespace LinqToDB.Tests.SearchEngine.TypeGraphEx
{
    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;
    using LinqToDB.Tests.SearchEngine.TypeGraphEx.Base;

    using Xunit;

    public class HierarchyPropertyTest : TypeGraphExBaseTest
    {
        public interface IBase
        {
        }

        public interface IA : IBase
        {
            [SearchContainer]
            IC C { get; set; }
        }

        public interface IB : IBase
        {
        }

        public interface IC : IB
        {
        }

        [Fact]
        public void Test()
        {
            var counter = 0;
            var baseVertex = new TypeVertex(typeof(IBase), counter++);
            var a = new TypeVertex(typeof(IA), counter++);
            var b = new TypeVertex(typeof(IB), counter++);
            var c = new TypeVertex(typeof(IC), counter++);

            var propAC = typeof(IA).GetProperty("C");
            var ac = new PropertyEdge(a, propAC, c);

            var expectedGraph = GetGraphArray(baseVertex, a, b, c);

            //// IBase -> []
            ////       ~> [IA, IB, IC]
            //// 
            ////    IA -> [{IA.C, IC}]
            ////       ~> [IBase]
            //// 
            ////    IB -> []
            ////       ~> [IBase, IC]
            //// 
            ////    IC -> []
            ////       ~> [IBase, IB]

            expectedGraph[baseVertex.Index].Casts.AddRange(new[] { new CastEdge(baseVertex, a), new CastEdge(baseVertex, b), new CastEdge(baseVertex, c) });

            expectedGraph[a.Index].Children.AddLast(ac);
            expectedGraph[a.Index].Casts.AddLast(new CastEdge(a, baseVertex));

            expectedGraph[b.Index].Casts.AddLast(new CastEdge(b, baseVertex));
            expectedGraph[b.Index].Casts.AddLast(new CastEdge(b, c));

            expectedGraph[c.Index].Parents.AddLast(ac);
            expectedGraph[c.Index].Casts.AddLast(new CastEdge(c, baseVertex));
            expectedGraph[c.Index].Casts.AddLast(new CastEdge(c, b));

            var typeGraph = BuildTypeGraph<IBase>();

            Assert.True(IsEqual(expectedGraph, typeGraph.Vertices));
        }
    }
}
