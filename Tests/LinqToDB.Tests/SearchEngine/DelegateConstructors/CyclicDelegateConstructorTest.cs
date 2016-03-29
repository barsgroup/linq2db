namespace LinqToDB.Tests.SearchEngine.DelegateConstructors
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.Search;
    using LinqToDB.SqlQuery.Search.PathBuilder;
    using LinqToDB.SqlQuery.Search.TypeGraph;

    using Xunit;

    public class CyclicDelegateConstructorTest
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
            var typeGraph = new TypeGraph<IBase>(GetType().Assembly.GetTypes());

            var pathBuilder = new PathBuilder<IBase>(typeGraph);

            var obj = new A();
            obj.B.C.A = new A();
            var paths = pathBuilder.Find(obj, typeof(IC));

            var delegateConstructor = new DelegateConstructor<IC>();
            var deleg = delegateConstructor.CreateResultDelegate(paths);

            LinkedList<IC> result = new LinkedList<IC>();
            deleg(obj, result);

            Assert.Equal(2, result.Count);
            Assert.Equal(obj.B.C, result.First.Value);
            Assert.Equal(obj.B.C.A.B.C, result.First.Next.Value);
        }
    }
}