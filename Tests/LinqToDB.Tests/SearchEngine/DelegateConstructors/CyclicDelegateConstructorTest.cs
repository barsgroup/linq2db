namespace LinqToDB.Tests.SearchEngine.DelegateConstructors
{
    using System.Collections.Generic;
    using System.Linq;

    using LinqToDB.SqlQuery.Search;
    using LinqToDB.SqlQuery.Search.PathBuilder;
    using LinqToDB.SqlQuery.Search.TypeGraph;
    using LinqToDB.Tests.SearchEngine.DelegateConstructors.Base;

    using Xunit;

    public class CyclicDelegateConstructorTest : BaseDelegateConstructorTest
    {
        private interface IBase
        {
        }

        private interface IA : IBase
        {
            [SearchContainer]
            IB B { get; set; } 
        }

        private interface IB : IBase
        {
            [SearchContainer]
            IC C { get; set; }
        }

        private interface IC : IBase
        {
            [SearchContainer]
            IA A { get; set; }
        }

        private class A : IA
        {
            public IB B { get; set; }

            public A()
            {
                B = new B();
            }
        }

        private class B : IB
        {
            public IC C { get; set; }

            public B()
            {
                C = new C();
            }
        }

        private class C : IC
        {
            public IA A { get; set; }
        }

        [Fact]
        public void SimpleDelegate()
        {
            CompareWithReflectionSearcher<IBase, IC>();
        }

        protected override object SetupTestObject()
        {
            var obj = new A();
            obj.B.C.A = new A();

            return obj;
        }
    }
}