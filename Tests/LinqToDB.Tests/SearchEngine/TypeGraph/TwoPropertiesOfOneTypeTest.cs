namespace LinqToDB.Tests.SearchEngine.TypeGraph
{
    using System;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;
    using LinqToDB.Tests.SearchEngine.TypeGraph.Base;

    using Xunit;

    public class TwoPropertiesOfOneTypeTest : TypeGraphBaseTest
    {
        public interface IBase
        {
            [SearchContainer]
            IA A1 { get; set; }

            [SearchContainer]
            IA A2 { get; set; }
        }

        public interface IA : IBase
        {
        }

        [Fact]
        public void Test()
        {
            var counter = 0;
            var baseVertex = new TypeVertex(typeof(IBase), counter++);
            var a = new TypeVertex(typeof(IA), counter++);

            var propBaseA1 = typeof(IBase).GetProperty("A1");
            var propBaseA2 = typeof(IBase).GetProperty("A2");

            var expectedGraph = GetGraphArray(baseVertex, a);

            //// IBase -> [{IBase.A1, IBase}, {IBase.A1, IA}, {IBase.A2, IBase}, {IBase.A2, IA}]
            //// IA -> []

            expectedGraph[baseVertex.Index].Children.AddRange(new[]
                                                                  {
                                                                      Tuple.Create(propBaseA1, baseVertex),
                                                                      Tuple.Create(propBaseA1, a),
                                                                      Tuple.Create(propBaseA2, baseVertex),
                                                                      Tuple.Create(propBaseA2, a)
                                                                  });

            var typeGraph = BuildTypeGraph<IBase>();

            Assert.True(IsEqual(expectedGraph, typeGraph.Vertices));
        }
    }
}
