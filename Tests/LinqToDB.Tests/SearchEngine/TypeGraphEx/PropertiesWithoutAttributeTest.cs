namespace LinqToDB.Tests.SearchEngine.TypeGraphEx
{
    using System;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search.SearchEx;
    using LinqToDB.Tests.SearchEngine.TypeGraph.Base;
    using LinqToDB.Tests.SearchEngine.TypeGraphEx.Base;

    using Xunit;

    using SearchContainerAttribute = LinqToDB.SqlQuery.Search.SearchContainerAttribute;

    public class PropertiesWithoutAttributeTest : TypeGraphExBaseTest
    {
        public interface IBase
        {
            [SearchContainer]
            IA A1 { get; set; }
            
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
            var basea = new PropertyEdge(baseVertex, propBaseA1, a);

            var expectedGraph = GetGraphArray(baseVertex, a);

            //// IBase -> [{IBase.A1, IA}]
            ////       ~> [IA]
            //// 
            //// IA    -> []
            ////       ~> [IBase]

            expectedGraph[baseVertex.Index].Children.AddLast(basea);
            expectedGraph[baseVertex.Index].Casts.AddLast(new CastEdge(baseVertex, a));

            expectedGraph[a.Index].Parents.AddLast(basea);
            expectedGraph[a.Index].Casts.AddLast(new CastEdge(a, baseVertex));

            var typeGraph = BuildTypeGraph<IBase>();

            Assert.True(IsEqual(expectedGraph, typeGraph.Vertices));
        }
    }
}
