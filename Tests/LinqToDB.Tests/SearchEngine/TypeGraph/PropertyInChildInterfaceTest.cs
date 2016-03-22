namespace LinqToDB.Tests.SearchEngine.TypeGraph
{
    using System;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;
    using LinqToDB.Tests.SearchEngine.TypeGraph.Base;

    using Xunit;

    public class PropertyInChildInterfaceTest : TypeGraphBaseTest
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

            var expectedGraph = GetGraphArray(baseVertex, a, b, c, d);

            //// IBase -> []
            //// IA -> [{IA.C, IBase}, {IA.C, IB}, {IA.C, IC}]
            //// IB -> []
            //// IC -> [{IC.D, IBase}, {IC.D, ID}]
            //// ID -> []
            
            expectedGraph[a.Index].Children.AddRange(new[] { Tuple.Create(propAC, baseVertex), Tuple.Create(propAC, b), Tuple.Create(propAC, c) });
            expectedGraph[c.Index].Children.AddRange(new[] { Tuple.Create(propCD, baseVertex), Tuple.Create(propCD, d) });

            var typeGraph = BuildTypeGraph<IBase>();

            Assert.True(IsEqual(expectedGraph, typeGraph.Vertices));
        }
    }
}
