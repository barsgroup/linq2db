namespace LinqToDB.Tests.SearchEngine.PathBuilder.Find
{
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
            //var typeGraph = new TypeGraph<IBase>(GetType().Assembly.GetTypes());
            //
            //var pathBuilder = new PathBuilder<IBase>(typeGraph);
            //
            //var result = pathBuilder.Find(new ClassA(), typeof(IF));
            //
            //var basea = typeof(IBase).GetProperty("A");
            //var baseb = typeof(IBase).GetProperty("B1");
            //var ab = typeof(IA).GetProperty("B2");
            //var ac = typeof(IA).GetProperty("C");
            //var bc = typeof(IB).GetProperty("C");
            //var cd = typeof(IC).GetProperty("D");
            //var ce = typeof(IC).GetProperty("E");
            //var cf = typeof(IC).GetProperty("F");
            //
            //var path1 = new CompositPropertyVertex();
            //path1.PropertyList.AddLast(ab);
            //path1.PropertyList.AddLast(bc);
            //path1.PropertyList.AddLast(cf);
            //
            //var path2 = new CompositPropertyVertex();
            //path2.PropertyList.AddLast(ac);
            //path2.PropertyList.AddLast(cf);
            //
            //var expected = new[] { path1, path2 };
            //
            //Assert.True(IsEqual(expected, result));
        }
    }
}
