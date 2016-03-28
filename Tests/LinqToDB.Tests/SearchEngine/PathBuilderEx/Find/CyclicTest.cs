namespace LinqToDB.Tests.SearchEngine.PathBuilderEx.Find
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using LinqToDB.SqlQuery.Search;
    using LinqToDB.Tests.SearchEngine.PathBuilderEx.Find.Base;

    using Xunit;

    public class CyclicTest : BaseFindTest
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
            [SearchContainer]
            IA A { get; set; }
        }

        public class ClassA : IA
        {
            public IB B { get; set; }
        }

        public class F : IF
        {
            public IA A { get; set; }
        }

        [Fact]
        public void Test()
        {
            var typeGraph = new TypeGraph<IBase>(GetType().Assembly.GetTypes());
            
            var pathBuilder = new PathBuilder<IBase>(typeGraph);
            
            var result = pathBuilder.Find(new ClassA(), typeof(IF));
            
            var dictionary = new Dictionary<PropertyInfo, CompositPropertyVertex>();

            Assert.True(CheckCyclicGraph(result, dictionary));
        }

        private bool CheckCyclicGraph(IEnumerable<CompositPropertyVertex> graph, Dictionary<PropertyInfo, CompositPropertyVertex> visited)
        {
            foreach (var vertex in graph)
            {
                var key = vertex.PropertyList.First.Value;

                if (visited.ContainsKey(key))
                {
                    return ReferenceEquals(visited[key], vertex);
                }

                if (vertex.PropertyList.Any(visited.ContainsKey))
                {
                    return false;
                }

                visited[key] = vertex;

                CheckCyclicGraph(vertex.Children, visited);
            }

            return true;
        }
    }
}
