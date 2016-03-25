namespace LinqToDB.Tests.SearchEngine.TypeGraph
{
    using System;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;
    using LinqToDB.Tests.SearchEngine.TypeGraph.Base;

    using Xunit;

    public class HierarchyPropertyTest : TypeGraphBaseTest
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

            var expectedGraph = GetGraphArray(baseVertex, a, b, c);
            
            //// IBase -> []
            //// IA -> [{IA.C, IBase}, {IA.C, IB}, {IA.C, IC}]
            //// IB -> []
            //// IC -> []

            expectedGraph[a.Index].Children.AddRange(new[] { new Edge(a, propAC, baseVertex), new Edge(a, propAC, b), new Edge(a, propAC, c) });

            var typeGraph = BuildTypeGraph<IBase>();

            Assert.True(IsEqual(expectedGraph, typeGraph.Vertices));
        }
    }
}
