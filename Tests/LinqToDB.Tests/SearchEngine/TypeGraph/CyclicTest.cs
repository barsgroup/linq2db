namespace LinqToDB.Tests.SearchEngine.TypeGraph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LinqToDB.SqlQuery.Search;
    using LinqToDB.Tests.SearchEngine.PathBuilder.Find.Base;

    using Xunit;

    public class CyclicTest : BaseFindTest
    {
        public interface IBase
        {
            [SearchContainer]
            IA A { get; set; }

            [SearchContainer]
            IB B1 { get; set; }
        }

        public interface IA : IBase
        {
            [SearchContainer]
            IB B2 { get; set; }

            [SearchContainer]
            IC C { get; set; }
        }

        public interface IB : IBase
        {
            [SearchContainer]
            IC C { get; set; }
        }

        public interface IC : IBase
        {
            [SearchContainer]
            ID D { get; set; }

            [SearchContainer]
            IE E { get; set; }

            [SearchContainer]
            IF F { get; set; }

            [SearchContainer]
            IBase FBase { get; set; }
        }

        public interface ID : IBase
        {
        }

        public interface IE : IBase
        {
        }

        public interface IF : IBase
        {
        }

        public class ClassA : IA
        {
            public IB B2 { get; set; }

            public IC C { get; set; }

            public IA A { get; set; }

            public IB B1 { get; set; }
        }

        public class F : IF
        {
            public IA A { get; set; }

            public IB B1 { get; set; }
        }

        [Fact]
        public void Test()
        {
            var typeGraph = new TypeGraph<IBase>(GetType().Assembly.GetTypes());
            
            Assert.True(CheckCyclicGraph(typeGraph.Vertices, new Dictionary<Type, TypeVertex>()));
        }

        private bool CheckCyclicGraph(IEnumerable<TypeVertex> graph, Dictionary<Type, TypeVertex> visited)
        {
            foreach (var vertex in graph)
            {
                var key = vertex.Type;

                if (visited.ContainsKey(key))
                {
                    return ReferenceEquals(visited[key], vertex);
                }

                visited[key] = vertex;

                CheckCyclicGraph(vertex.Children.Select(c => c.Child), visited);
            }

            return true;
        }
    }
}
