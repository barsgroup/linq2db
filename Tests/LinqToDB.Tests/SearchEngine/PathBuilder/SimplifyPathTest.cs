namespace LinqToDB.Tests.SearchEngine.PathBuilder
{
    using System.Collections.Generic;
    using System.Linq;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;

    using Xunit;

    public class SimplifyPathTest
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

        [Fact]
        public void NoChildrenTest()
        {
            var ab = typeof(IA).GetProperty("B");
            var ac = typeof(IA).GetProperty("C");

            var vertexAB = new PropertyInfoVertex(ab);
            var vertexAC = new PropertyInfoVertex(ac);

            var origin = new LinkedList<PropertyInfoVertex>();
            origin.AddRange(new[]
                                {
                                    vertexAB, vertexAB,
                                    vertexAC, vertexAC,
                                    vertexAB, vertexAB, vertexAB,
                                    vertexAC
                                });
            
            var result = PathBuilder<IBase>.Simplify(origin);

            var resultArray = result.OrderBy(v => v.Property.Name).ToArray();

            var expectedResultArray = new[] { vertexAB, vertexAC };

            Assert.Equal(expectedResultArray.Length, resultArray.Length);

            for (var i = 0; i < resultArray.Length; ++i)
            {
                Assert.Equal(expectedResultArray[i].Property, resultArray[i].Property);
            }
        }

        [Fact]
        public void SameChildrenTest()
        {
            var ab = typeof(IA).GetProperty("B");
            var bc = typeof(IB).GetProperty("C");

            var vertexAB = new PropertyInfoVertex(ab);
            var vertexBC = new PropertyInfoVertex(bc);

            vertexAB.Children.AddRange(new[] { vertexBC, vertexBC, vertexBC });

            var origin = new LinkedList<PropertyInfoVertex>();
            origin.AddRange(new[]
                                {
                                    vertexAB, vertexAB, vertexAB
                                });

            var result = PathBuilder<IBase>.Simplify(origin);

            var expectedVertexAB = new PropertyInfoVertex(ab);
            expectedVertexAB.Children.AddLast(new PropertyInfoVertex(bc));

            var expectedResult = new[] { expectedVertexAB };

            Assert.True(IsEqual(expectedResult, result));
        }

        [Fact]
        public void DifferentChildren()
        {
            var ab = typeof(IA).GetProperty("B");
            var ac = typeof(IA).GetProperty("C");
            var bc = typeof(IB).GetProperty("C");
            var cd = typeof(IC).GetProperty("D");
            var ce = typeof(IC).GetProperty("E");
            var cf = typeof(IC).GetProperty("F");

            var vertexCD = new PropertyInfoVertex(cd);
            var vertexCE = new PropertyInfoVertex(ce);
            var vertexCF = new PropertyInfoVertex(cf);

            var vertexBC0 = new PropertyInfoVertex(bc);
            var vertexBC1 = new PropertyInfoVertex(bc);
            vertexBC1.Children.AddLast(vertexCE);
            var vertexBC2 = new PropertyInfoVertex(bc);
            vertexBC2.Children.AddLast(vertexCD);
            vertexBC2.Children.AddLast(vertexCE);
            vertexBC2.Children.AddLast(vertexCF);
            var vertexBC3 = new PropertyInfoVertex(bc);
            vertexBC3.Children.AddLast(vertexCD);
            vertexBC3.Children.AddLast(vertexCE);
            vertexBC3.Children.AddLast(vertexCF);
            var vertexBC4 = new PropertyInfoVertex(bc);
            vertexBC4.Children.AddLast(vertexCE);
            vertexBC4.Children.AddLast(vertexCF);

            var vertexAC0 = new PropertyInfoVertex(ac);
            var vertexAC1 = new PropertyInfoVertex(ac);
            vertexAC1.Children.AddLast(vertexCD);
            var vertexAC2 = new PropertyInfoVertex(ac);
            vertexAC2.Children.AddLast(vertexCD);
            vertexAC2.Children.AddLast(vertexCE);
            vertexAC2.Children.AddLast(vertexCF);
            var vertexAC3 = new PropertyInfoVertex(ac);
            vertexAC3.Children.AddLast(vertexCD);
            vertexAC3.Children.AddLast(vertexCE);
            vertexAC3.Children.AddLast(vertexCF);

            var vertexAB1 = new PropertyInfoVertex(ab);
            vertexAB1.Children.AddLast(vertexBC0);
            vertexAB1.Children.AddLast(vertexBC1);
            vertexAB1.Children.AddLast(vertexBC2);
            var vertexAB2 = new PropertyInfoVertex(ab);
            vertexAB2.Children.AddLast(vertexBC0);
            vertexAB2.Children.AddLast(vertexBC1);
            vertexAB2.Children.AddLast(vertexBC2);
            vertexAB2.Children.AddLast(vertexBC3);
            vertexAB2.Children.AddLast(vertexBC4);
            var vertexAB3 = new PropertyInfoVertex(ab);
            vertexAB3.Children.AddLast(vertexBC4);
            vertexAB3.Children.AddLast(vertexBC4);
            vertexAB3.Children.AddLast(vertexBC4);

            var origin = new LinkedList<PropertyInfoVertex>();
            origin.AddRange(new[]
                                {
                                    vertexAB1, vertexAB2, vertexAB3,
                                    vertexAC0, vertexAC1, vertexAC2, vertexAC3
                                });

            var result = PathBuilder<IBase>.Simplify(origin);

            var expectedVertexBC = new PropertyInfoVertex(bc);
            expectedVertexBC.Children.AddRange(new[]
                                           {
                                               vertexCD, vertexCE, vertexCF
                                           });

            var expectedVertexAC = new PropertyInfoVertex(ac);
            expectedVertexAC.Children.AddRange(new[]
                                           {
                                               vertexCD, vertexCE, vertexCF
                                           });

            var expectedVertexAB = new PropertyInfoVertex(ab);
            expectedVertexAB.Children.AddLast(expectedVertexBC);

            var expectedResult = new[] { expectedVertexAB, expectedVertexAC };

            Assert.True(IsEqual(expectedResult, result));
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
