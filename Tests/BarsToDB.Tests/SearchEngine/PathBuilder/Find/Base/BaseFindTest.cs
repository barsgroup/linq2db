namespace LinqToDB.Tests.SearchEngine.PathBuilder.Find.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using LinqToDB.SqlQuery.Search;
    using LinqToDB.SqlQuery.Search.PathBuilder;

    public class BaseFindTest
    {
        protected bool IsEqual(IEnumerable<CompositPropertyVertex> graph1, IEnumerable<CompositPropertyVertex> graph2)
        {
            return IsEqualInternal(graph1, graph2, new Dictionary<Tuple<CompositPropertyVertex, CompositPropertyVertex>, bool>());
        }

        protected bool IsEqualInternal(IEnumerable<CompositPropertyVertex> graph1, IEnumerable<CompositPropertyVertex> graph2, Dictionary<Tuple<CompositPropertyVertex, CompositPropertyVertex>, bool> isEqualCache)
        {
            var array1 = GetOrderedArray(graph1);
            var array2 = GetOrderedArray(graph2);

            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (var i = 0; i < array1.Length; ++i)
            {
                var node1 = array1[i];
                var node2 = array2[i];
                var key = Tuple.Create(node1, node2);

                bool result;
                if (isEqualCache.TryGetValue(key, out result))
                {
                    if (!result)
                    {
                        return false;
                    }

                    continue;
                }

                isEqualCache[key] = true;

                if (!IsEqual(node1.PropertyList, node2.PropertyList))
                {
                    isEqualCache[key] = false;
                    return false;
                }

                if (!IsEqualInternal(node1.Children, node2.Children, isEqualCache))
                {
                    isEqualCache[key] = false;
                    return false;
                }
            }

            return true;
        }

        private bool IsEqual(IEnumerable<PropertyInfo> list1, IEnumerable<PropertyInfo> list2)
        {
            var array1 = list1.ToArray();
            var array2 = list2.ToArray();

            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (var i = 0; i < array1.Length; ++i)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }

        private CompositPropertyVertex[] GetOrderedArray(IEnumerable<CompositPropertyVertex> vertices)
        {
            return vertices.OrderBy(v => v.ToString()).ToArray();
        }
    }
}
