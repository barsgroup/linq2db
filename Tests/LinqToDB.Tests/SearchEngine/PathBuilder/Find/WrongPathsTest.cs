namespace LinqToDB.Tests.SearchEngine.PathBuilder.Find
{
    using LinqToDB.SqlQuery.Search;
    using LinqToDB.Tests.SearchEngine.PathBuilder.Find.Base;

    using Xunit;

    public class WrongPathsTest : BaseFindTest
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
        }

        public interface IC : IBase
        {
            [SearchContainer]
            ID D { get; set; }
        }

        public interface ID : IBase
        {
        }

        public interface ICDerived : IC
        {
            [SearchContainer]
            IB B { get; set; }
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

            var result = pathBuilder.Find(new A(), typeof(IB));

            var ab = typeof(IA).GetProperty("B");
            var ac = typeof(IA).GetProperty("C");
            var cderb = typeof(ICDerived).GetProperty("B");

            var vertex1 = new CompositPropertyVertex();
            vertex1.PropertyList.AddLast(ab);

            var vertex2 = new CompositPropertyVertex();
            vertex2.PropertyList.AddLast(ac);
            vertex2.PropertyList.AddLast(cderb);
            var expected = new[] { vertex1, vertex2 };

            Assert.True(IsEqual(expected, result));
        }
    }
}
