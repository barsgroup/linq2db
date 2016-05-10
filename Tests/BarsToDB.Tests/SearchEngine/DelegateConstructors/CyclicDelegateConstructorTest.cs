using Bars2Db.SqlQuery.Search;
using LinqToDB.Tests.SearchEngine.DelegateConstructors.Base;
using Xunit;

namespace LinqToDB.Tests.SearchEngine.DelegateConstructors
{
    public class CyclicDelegateConstructorTest : BaseDelegateConstructorTest<CyclicDelegateConstructorTest.IBase>
    {
        public interface IBase
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
            public A()
            {
                B = new B();
            }

            public IB B { get; set; }
        }

        private class B : IB
        {
            public B()
            {
                C = new C();
            }

            public IC C { get; set; }
        }

        private class C : IC
        {
            public IA A { get; set; }
        }

        protected IBase SetupTestObject()
        {
            var obj = new A();
            obj.B.C.A = new A();
            obj.B.C.A.B.C.A = new A();

            return obj;
        }

        [Fact]
        public void SimpleDelegate()
        {
            var obj = SetupTestObject();

            Assert.True(CompareWithReflectionSearcher<IA>(obj));
            Assert.True(CompareWithReflectionSearcher<IB>(obj));
            Assert.True(CompareWithReflectionSearcher<IC>(obj));
        }
    }
}