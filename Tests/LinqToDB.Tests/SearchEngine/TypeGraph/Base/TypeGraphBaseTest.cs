namespace LinqToDB.Tests.SearchEngine.TypeGraph.Base
{
    using System;
    using System.Linq;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;
    using LinqToDB.SqlQuery.Search.TypeGraph;

    public class TypeGraphBaseTest
    {
        protected static TypeVertex[] GetGraphArray(params TypeVertex[] vertices)
        {
            var result = vertices.OrderBy(v => v.Index).ToArray();

            if (result.Where((v, i) => v.Index != i).Any())
            {
                throw new Exception("Bad indices");
            }

            return result;
        }

        protected static TypeGraph<T> BuildTypeGraph<T>()
        {
            return new TypeGraph<T>(typeof(T).Assembly.GetTypes());
        }

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

                if (vertex1.Parents.Count != vertex2.Parents.Count)
                {
                    return false;
                }

                var parents1 = vertex1.Parents.ToArray();
                var parents2 = vertex2.Parents.ToArray();

                for (var j = 0; j < parents1.Length; ++j)
                {
                    if (!parents1[j].Equals(parents2[j]))
                    {
                        return false;
                    }

                    ////var parent1 = parents1[j];
                    ////var parent2 = parents2[j];
                    ////
                    ////if (parent1.PropertyInfo != parent2.PropertyInfo)
                    ////{
                    ////    return false;
                    ////}
                    ////
                    ////if (parent1.Parent.Type != parent2.Parent.Type)
                    ////{
                    ////    return false;
                    ////}
                    ////
                    ////if (parent1.Child.Type != parent2.Child.Type)
                    ////{
                    ////    return false;
                    ////}
                }

                if (vertex1.Children.Count != vertex2.Children.Count)
                {
                    return false;
                }

                var children1 = vertex1.Children.ToArray();
                var children2 = vertex2.Children.ToArray();

                for (var j = 0; j < children1.Length; ++j)
                {
                    if (!children1[j].Equals(children2[j]))
                    {
                        return false;
                    }

                    ////var child1 = children1[j];
                    ////var child2 = children2[j];
                    ////
                    ////if (child1.PropertyInfo != child2.PropertyInfo)
                    ////{
                    ////    return false;
                    ////}
                    ////
                    ////if (child1.Parent.Type != child2.Parent.Type)
                    ////{
                    ////    return false;
                    ////}
                    ////
                    ////if (child1.Child.Type != child2.Child.Type)
                    ////{
                    ////    return false;
                    ////}
                }

                if (vertex1.Casts.Count != vertex2.Casts.Count)
                {
                    return false;
                }

                var casts1 = vertex1.Casts.ToArray();
                var casts2 = vertex2.Casts.ToArray();

                for (var j = 0; j < casts1.Length; ++j)
                {
                    if (!casts1[j].Equals(casts2[j]))
                    {
                        return false;
                    }

                    ////if (casts1[j].CastFrom.Type != casts2[j].CastFrom.Type)
                    ////{
                    ////    return false;
                    ////}
                    ////
                    ////if (casts1[j].CastTo.Type != casts2[j].CastTo.Type)
                    ////{
                    ////    return false;
                    ////}
                }
            }

            return true;
        }

        private static TypeVertex[] PrepareGraph(TypeVertex[] graph)
        {
            graph = graph.OrderBy(v => v.Type.FullName).ToArray();
            foreach (var vertex in graph)
            {
                var newChildren =
                    vertex.Children.OrderBy(child => child.PropertyInfo.Name)
                          .ThenBy(child => child.PropertyInfo.DeclaringType.FullName)
                          .ThenBy(child => child.PropertyInfo.ReflectedType.FullName)
                          .ThenBy(child => child.PropertyInfo.PropertyType.FullName)
                          .ThenBy(child => child.Child.Type.FullName)
                          .ToList();
                vertex.Children.Clear();
                vertex.Children.AddRange(newChildren);

                var newCasts = vertex.Casts.OrderBy(child => child.CastTo.Type.FullName).ToList();
                vertex.Casts.Clear();
                vertex.Casts.AddRange(newCasts);
            }

            return graph;
        }
    }
}