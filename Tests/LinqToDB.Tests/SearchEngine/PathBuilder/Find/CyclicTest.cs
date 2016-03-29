namespace LinqToDB.Tests.SearchEngine.PathBuilder.Find
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    
    using LinqToDB.SqlQuery.Search.PathBuilder;
    using LinqToDB.SqlQuery.Search.TypeGraph;
    using LinqToDB.Tests.SearchEngine.PathBuilder.Find.Base;
    using LinqToDB.Tests.SearchEngine.TestInterfaces.CyclicDependency;

    using Xunit;

    public class CyclicTest : BaseFindTest
    {
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
