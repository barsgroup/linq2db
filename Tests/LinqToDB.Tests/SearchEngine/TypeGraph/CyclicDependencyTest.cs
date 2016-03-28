namespace LinqToDB.Tests.SearchEngine.TypeGraph
{
    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;
    using LinqToDB.Tests.SearchEngine.TypeGraph.Base;

    using Xunit;

    public class CyclicDependencyTest : TypeGraphBaseTest
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
            IC C { get; set; }
        }

        public interface IC : IBase
        {
            [SearchContainer]
            IA A { get; set; }
        }

        [Fact]
        public void Test()
        {
            var counter = 0;
            var baseVertex = new TypeVertex(typeof(IBase), counter++);
            var a = new TypeVertex(typeof(IA), counter++);
            var b = new TypeVertex(typeof(IB), counter++);
            var c = new TypeVertex(typeof(IC), counter++);

            var propAB = typeof(IA).GetProperty("B");
            var propBC = typeof(IB).GetProperty("C");
            var propCA = typeof(IC).GetProperty("A");

            var ab = new PropertyEdge(a, propAB, b);
            var bc = new PropertyEdge(b, propBC, c);
            var ca = new PropertyEdge(c, propCA, a);

            var expectedGraph = GetGraphArray(baseVertex, a, b, c);

            //// IBase -> []
            ////       ~> [IA, IB, IC]
            //// 
            ////    IA -> [{IA.B, IB}]
            ////       ~> [IBase]
            ////    
            ////    IB -> [{IB.C, IC}]
            ////       ~> [IBase]
            ////    
            ////    IC -> [{IC.A, IA}]
            ////       ~> [IBase]

            expectedGraph[baseVertex.Index].Casts.AddRange(new[] { new CastEdge(baseVertex, a), new CastEdge(baseVertex, b), new CastEdge(baseVertex, c) });

            expectedGraph[a.Index].Parents.AddLast(ca);
            expectedGraph[a.Index].Children.AddLast(ab);
            expectedGraph[a.Index].Casts.AddLast(new CastEdge(a, baseVertex));

            expectedGraph[b.Index].Parents.AddLast(ab);
            expectedGraph[b.Index].Children.AddLast(bc);
            expectedGraph[b.Index].Casts.AddLast(new CastEdge(b, baseVertex));

            expectedGraph[c.Index].Parents.AddLast(bc);
            expectedGraph[c.Index].Children.AddLast(ca);
            expectedGraph[c.Index].Casts.AddLast(new CastEdge(c, baseVertex));

            var typeGraph = BuildTypeGraph<IBase>();

            Assert.True(IsEqual(expectedGraph, typeGraph.Vertices));
        }
    }
}
