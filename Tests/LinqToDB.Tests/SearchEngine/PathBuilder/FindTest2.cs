namespace LinqToDB.Tests.SearchEngine.PathBuilder
{
    using System.Collections.Generic;
    using System.Linq;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;

    using Xunit;

    public class FindTest2
    {
        public interface IBase
        {
        }

        public interface IA : IBase
        {
            [SearchContainer]
            IB B { get; set; }

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

        public class A : IA
        {
            public IB B { get; set; }

            public IC C { get; set; }
        }

        [Fact]
        public void Test()
        {
            var typeGraph = new TypeGraph<IBase>(GetType().Assembly.GetTypes());

            var pathBuilder = new PathBuilder<IBase>(typeGraph);

            var result = pathBuilder.Find(new A(), typeof(IF));

            Assert.True(result.Any());
        }

        private bool IsEqual(IEnumerable<PropertyInfoVertex> graph1, IEnumerable<PropertyInfoVertex> graph2)
        {
            var array1 = GetOrderedArray(graph1);
            var array2 = GetOrderedArray(graph2);

            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (var i = 0; i < array1.Length; ++i)
            {
                if (!IsEqual(array1[i].Children, array2[i].Children))
                {
                    return false;
                }
            }

            return true;
        }

        private PropertyInfoVertex[] GetOrderedArray(IEnumerable<PropertyInfoVertex> vertices)
        {
            return vertices.OrderBy(v => v.Property.Name)
                      .ThenBy(v => v.Property.DeclaringType.FullName)
                      .ThenBy(v => v.Property.ReflectedType.FullName)
                      .ThenBy(v => v.Property.PropertyType.FullName).ToArray();
        }
    }
}
