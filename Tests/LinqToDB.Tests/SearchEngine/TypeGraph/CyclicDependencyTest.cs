namespace LinqToDB.Tests.SearchEngine.TypeGraph
{
    using System;

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

            var expectedGraph = GetGraphArray(baseVertex, a, b, c);

            //// IBase -> []
            //// IA -> [{IA.B, IBase}, {IA.B, IB}]
            //// IB -> [{IB.C, IBase}, {IB.C, IC}]
            //// IC -> [{IC.A, IBase}, {IC.A, IA}]

            expectedGraph[a.Index].Children.AddRange(new[] { Tuple.Create(propAB, baseVertex), Tuple.Create(propAB, b) });
            expectedGraph[b.Index].Children.AddRange(new[] { Tuple.Create(propBC, baseVertex), Tuple.Create(propBC, c) });
            expectedGraph[c.Index].Children.AddRange(new[] { Tuple.Create(propCA, baseVertex), Tuple.Create(propCA, a) });

            var typeGraph = BuildTypeGraph<IBase>();

            Assert.True(IsEqual(expectedGraph, typeGraph.Vertices));
        }
    }
}
