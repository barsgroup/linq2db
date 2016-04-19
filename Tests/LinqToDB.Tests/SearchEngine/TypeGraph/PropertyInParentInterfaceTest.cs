namespace LinqToDB.Tests.SearchEngine.TypeGraphEx
{
    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;
    using LinqToDB.SqlQuery.Search.TypeGraph;
    using LinqToDB.Tests.SearchEngine.TypeGraph.Base;

    using Xunit;

    public class PropertyInParentInterfaceTest : TypeGraphBaseTest
    {
        public interface IBase
        {
        }

        public interface IA : IBase
        {
            [SearchContainer]
            IB B { get; set; }
        }

        public interface IB : IBase
        {
            [SearchContainer]
            ID D { get; set; }
        }

        public interface IC : IB
        {
        }

        public interface ID : IBase
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
            var d = new TypeVertex(typeof(ID), counter++);

            var propAB = typeof(IA).GetProperty("B");
            var propBD = typeof(IB).GetProperty("D");

            var ab = new PropertyEdge(a, propAB, b);
            var bd = new PropertyEdge(b, propBD, d);

            var expectedGraph = GetGraphArray(baseVertex, a, b, c, d);

            //// IBase -> []
            ////       ~> [IA, IB, IC, ID]
            //// 
            ////    IA -> [{IA.C, IB}]
            ////       ~> [IBase]
            //// 
            ////    IB -> [{IB.D, ID}]
            ////       ~> [IBase, IC]
            //// 
            ////    IC -> []
            ////       ~> [IBase, IB]
            //// 
            ////    ID -> []
            ////       ~> [IBase]

            expectedGraph[baseVertex.Index].Casts.AddRange(new[] { new CastEdge(baseVertex, a), new CastEdge(baseVertex, b), new CastEdge(baseVertex, c), new CastEdge(baseVertex, d) });

            expectedGraph[a.Index].Children.AddLast(ab);
            expectedGraph[a.Index].Casts.AddLast(new CastEdge(a, baseVertex));

            expectedGraph[b.Index].Parents.AddLast(ab);
            expectedGraph[b.Index].Children.AddLast(bd);
            expectedGraph[b.Index].Casts.AddLast(new CastEdge(b, baseVertex));
            expectedGraph[b.Index].Casts.AddLast(new CastEdge(b, c));

            expectedGraph[c.Index].Casts.AddLast(new CastEdge(c, baseVertex));
            expectedGraph[c.Index].Casts.AddLast(new CastEdge(c, b));

            expectedGraph[d.Index].Parents.AddLast(bd);
            expectedGraph[d.Index].Casts.AddLast(new CastEdge(d, baseVertex));

            var typeGraph = BuildTypeGraph<IBase>();

            Assert.True(IsEqual(expectedGraph, typeGraph.Vertices));
        }
    }
}
