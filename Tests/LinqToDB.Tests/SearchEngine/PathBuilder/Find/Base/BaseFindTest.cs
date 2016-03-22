namespace LinqToDB.Tests.SearchEngine.PathBuilder.Find.Base
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;

    public class BaseFindTest
    {
        protected bool IsEqual(IEnumerable<CompositPropertyVertex> graph1, IEnumerable<CompositPropertyVertex> graph2)
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

                if (!IsEqual(node1.PropertyList, node2.PropertyList))
                {
                    return false;
                }

                if (!IsEqual(node1.Children, node2.Children))
                {
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
            return vertices.OrderBy(
                v =>
                    {
                        var key = string.Empty;
                        v.PropertyList.ForEach(
                            node =>
                                {
                                    key += node.Value.DeclaringType.FullName + "." + node.Value.Name + "->";
                                });

                        return key;
                    }).ToArray();
        }
    }
}
