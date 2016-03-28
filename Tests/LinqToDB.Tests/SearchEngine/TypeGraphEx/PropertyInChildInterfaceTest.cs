namespace LinqToDB.Tests.SearchEngine.TypeGraphEx
{
    using System;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search.SearchEx;
    using LinqToDB.Tests.SearchEngine.TypeGraph.Base;
    using LinqToDB.Tests.SearchEngine.TypeGraphEx.Base;

    using Xunit;

    using SearchContainerAttribute = LinqToDB.SqlQuery.Search.SearchContainerAttribute;

    public class PropertyInChildInterfaceTest : TypeGraphExBaseTest
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
            [SearchContainer]
            ID D { get; set; }
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

            var propAC = typeof(IA).GetProperty("C");
            var propCD = typeof(IC).GetProperty("D");

            var ac = new PropertyEdge(a, propAC, c);
            var cd = new PropertyEdge(c, propCD, d);

            var expectedGraph = GetGraphArray(baseVertex, a, b, c, d);

            //// IBase -> []
            ////       ~> [IA, IB, IC, ID]
            //// 
            ////    IA -> [{IA.C, IC}]
            ////       ~> [IBase]
            //// 
            ////    IB -> []
            ////       ~> [IBase, IC]
            //// 
            ////    IC -> [{IC.D, ID}]
            ////       ~> [IBase, IB]
            //// 
            ////    ID -> []
            ////       ~> [IBase]

            expectedGraph[baseVertex.Index].Casts.AddRange(new[] { new CastEdge(baseVertex, a), new CastEdge(baseVertex, b), new CastEdge(baseVertex, c), new CastEdge(baseVertex, d) });

            expectedGraph[a.Index].Children.AddLast(ac);
            expectedGraph[a.Index].Casts.AddLast(new CastEdge(a, baseVertex));

            expectedGraph[b.Index].Casts.AddLast(new CastEdge(b, baseVertex));
            expectedGraph[b.Index].Casts.AddLast(new CastEdge(b, c));

            expectedGraph[c.Index].Parents.AddLast(ac);
            expectedGraph[c.Index].Children.AddLast(cd);
            expectedGraph[c.Index].Casts.AddLast(new CastEdge(c, baseVertex));
            expectedGraph[c.Index].Casts.AddLast(new CastEdge(c, b));

            expectedGraph[d.Index].Parents.AddLast(cd);
            expectedGraph[d.Index].Casts.AddLast(new CastEdge(d, baseVertex));

            var typeGraph = BuildTypeGraph<IBase>();

            Assert.True(IsEqual(expectedGraph, typeGraph.Vertices));
        }
    }
}
