namespace LinqToDB.Tests.SearchEngine.GraphBuilder
{
    using System;
    using System.Linq;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;

    using Xunit;

    public class Test1 : GraphBuilderBaseTest
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
            //// IBase -> []
            //// IA -> [{IA.C, IBase}, {IA.C, IB}, {IA.C, IC}]
            //// IB -> []
            //// IC -> [{IC.D, IBase}, {IC.D, ID}]
            //// ID -> []

            var counter = 0;
            var baseVertex = new TypeVertex(typeof(IBase), counter++);
            var a = new TypeVertex(typeof(IA), counter++);
            var b = new TypeVertex(typeof(IB), counter++);
            var c = new TypeVertex(typeof(IC), counter++);
            var d = new TypeVertex(typeof(ID), counter++);

            var piC = typeof(IA).GetProperty("C");
            var piD = typeof(IC).GetProperty("D");

            var expectedGraph = new[] { baseVertex, a, b, c, d };
            expectedGraph[1].Children.AddRange(new[] { Tuple.Create(piC, baseVertex), Tuple.Create(piC, b), Tuple.Create(piC, c) });
            expectedGraph[3].Children.AddRange(new[] { Tuple.Create(piD, baseVertex), Tuple.Create(piD, d) });

            var typeGraph = new TypeGraph<IBase>(GetType().Assembly.GetTypes());

            Assert.True(IsEqual(expectedGraph, typeGraph.Vertices));
        }
    }
}
