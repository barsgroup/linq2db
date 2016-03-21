namespace LinqToDB.Tests.SearchEngine.GraphBuilder
{
    using System;
    using System.Linq;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;

    using Xunit;

    public class HierarchyPropertyTest : GraphBuilderBaseTest
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
            //// IBase -> []
            //// IA -> [{IA.C, IBase}, {IA.C, IB}, {IA.C, IC}]
            //// IB -> []
            //// IC -> []

            var baseVertex = new TypeVertex(typeof(IBase), 0);
            var a = new TypeVertex(typeof(IA), 1);
            var b = new TypeVertex(typeof(IB), 2);
            var c = new TypeVertex(typeof(IC), 3);

            var propertyInfo = typeof(IA).GetProperty("C");

            var expectedGraph = new[] { baseVertex, a, b, c };
            expectedGraph[1].Children.AddRange(new[] { Tuple.Create(propertyInfo, baseVertex), Tuple.Create(propertyInfo, b), Tuple.Create(propertyInfo, c) });

            var typeGraph = new TypeGraph<IBase>(GetType().Assembly.GetTypes());

            Assert.True(IsEqual(expectedGraph, typeGraph.Vertices));
        }
    }
}
