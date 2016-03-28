namespace LinqToDB.Tests.SearchEngine.PathBuilderEx.Find
{
    using LinqToDB.SqlQuery.Search.SearchEx;
    using LinqToDB.Tests.SearchEngine.PathBuilderEx.Find.Base;

    using Xunit;

    using SearchContainerAttribute = LinqToDB.SqlQuery.Search.SearchContainerAttribute;

    public class OptimizeTest : BaseFindTest
    {
        public interface IBase
        {
        }

        public interface IBaseA : IBase
        {
            [SearchContainer]
            IA A { get; set; }
        }

        public interface IA : IBaseA
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
        }

        public interface ID : IBase
        {
            [SearchContainer]
            IE E { get; set; }
        }

        public interface IBaseE : IBase
        {
        }

        public interface IE : IBaseE
        {
            [SearchContainer]
            IBaseE BaseE { get; set; }

            [SearchContainer]
            IDerivedE DerivedE { get; set; }
        }

        public interface IDerivedE : IE
        {
        }

        public class ClassA : IA
        {
            public IA A { get; set; }

            public IB B { get; set; }
        }

        [Fact]
        public void Test()
        {
            var typeGraph = new TypeGraphEx<IBase>(GetType().Assembly.GetTypes());
            
            var pathBuilder = new PathBuilderEx<IBase>(typeGraph);
            
            var result = pathBuilder.Find(new ClassA(), typeof(IE));

            //// 1) IA.B -> IB.C -> IC.D -> ID.E
            ////                             |-> IE.BaseE
            ////                                    |-> IE.BaseE ...
            ////                                    |-> IE.DerivedE ...
            ////                             |-> IE.DerivedE 
            ////                                    |-> IE.BaseE ...
            ////                                    |-> IE.DerivedE ...
            ////
            //// 2) IBaseA.A
            ////        |-> IBaseA.A ...
            ////        |-> IA.B -> IB.C -> IC.D -> ID.E ...

            var ab = typeof(IA).GetProperty("B");
            var bc = typeof(IB).GetProperty("C");
            var cd = typeof(IC).GetProperty("D");
            var de = typeof(ID).GetProperty("E");
            var baseaA = typeof(IBaseA).GetProperty("A");
            var eBase = typeof(IE).GetProperty("BaseE");
            var eDerived = typeof(IE).GetProperty("DerivedE");

            var vertexABCDE = new CompositPropertyVertex();
            vertexABCDE.PropertyList.AddLast(ab);
            vertexABCDE.PropertyList.AddLast(bc);
            vertexABCDE.PropertyList.AddLast(cd);
            vertexABCDE.PropertyList.AddLast(de);
            var vertexBaseAA = new CompositPropertyVertex();
            vertexBaseAA.PropertyList.AddLast(baseaA);
            var vertexEBaseE = new CompositPropertyVertex();
            vertexEBaseE.PropertyList.AddLast(eBase);
            var vertexEDerivedE = new CompositPropertyVertex();
            vertexEDerivedE.PropertyList.AddLast(eDerived);

            vertexABCDE.Children.AddLast(vertexEBaseE);
            vertexABCDE.Children.AddLast(vertexEDerivedE);

            vertexEBaseE.Children.AddLast(vertexEBaseE);
            vertexEBaseE.Children.AddLast(vertexEDerivedE);

            vertexEDerivedE.Children.AddLast(vertexEBaseE);
            vertexEDerivedE.Children.AddLast(vertexEDerivedE);

            vertexBaseAA.Children.AddLast(vertexABCDE);
            vertexBaseAA.Children.AddLast(vertexBaseAA);

            var expected = new[] { vertexABCDE, vertexBaseAA };

            Assert.True(IsEqual(expected, result));
        }
    }
}
