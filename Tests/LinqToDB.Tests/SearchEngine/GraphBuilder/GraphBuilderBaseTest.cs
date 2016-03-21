namespace LinqToDB.Tests.SearchEngine.GraphBuilder
{
    using System.Linq;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;

    public class GraphBuilderBaseTest
    {
        protected static bool IsEqual(TypeVertex[] graph1, TypeVertex[] graph2)
        {
            if (graph1.Length != graph2.Length)
            {
                return false;
            }

            graph1 = PrepareGraph(graph1);
            graph2 = PrepareGraph(graph2);

            for (var i = 0; i < graph1.Length; ++i)
            {
                var vertex1 = graph1[i];
                var vertex2 = graph2[i];

                if (vertex1.Type != vertex2.Type)
                {
                    return false;
                }

                if (vertex1.Children.Count != vertex2.Children.Count)
                {
                    return false;
                }

                var children1 = vertex1.Children.ToArray();
                var children2 = vertex2.Children.ToArray();

                for (var j = 0; j < children1.Length; ++j)
                {
                    var child1 = children1[j];
                    var child2 = children2[j];

                    if (child1.Item1 != child2.Item1)
                    {
                        return false;
                    }

                    if (child1.Item2.Type != child2.Item2.Type)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static TypeVertex[] PrepareGraph(TypeVertex[] graph)
        {
            graph = graph.OrderBy(v => v.Type.FullName).ToArray();
            foreach (var vertex in graph)
            {
                var newChildren = vertex.Children
                    .OrderBy(child => child.Item1.Name)
                    .ThenBy(child => child.Item1.DeclaringType.FullName)
                    .ThenBy(child => child.Item1.ReflectedType.FullName)
                    .ThenBy(child => child.Item1.PropertyType.FullName)
                    .ThenBy(child => child.Item2.Type.FullName).ToList();
                vertex.Children.Clear();
                vertex.Children.AddRange(newChildren);
            }

            return graph;
        }
    }
}