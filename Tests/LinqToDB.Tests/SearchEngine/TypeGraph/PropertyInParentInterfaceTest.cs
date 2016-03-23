namespace LinqToDB.Tests.SearchEngine.TypeGraph
{
    using System;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;
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

            var expectedGraph = GetGraphArray(baseVertex, a, b, c, d);

            //// IBase -> []
            //// IA -> [{IA.C, IBase}, {IA.C, IB}]
            //// IB -> [{IB.D, IBase}, {IB.D, ID}]
            //// IC -> []
            //// ID -> []

            expectedGraph[a.Index].Children.AddRange(new[] { new Edge(a, propAB, baseVertex), new Edge(a, propAB, b) });
            expectedGraph[b.Index].Children.AddRange(new[] { new Edge(b, propBD, baseVertex), new Edge(b, propBD, d) });

            var typeGraph = BuildTypeGraph<IBase>();

            Assert.True(IsEqual(expectedGraph, typeGraph.Vertices));
        }
    }
}
