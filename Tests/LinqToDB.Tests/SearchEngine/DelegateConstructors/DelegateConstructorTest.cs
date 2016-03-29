namespace LinqToDB.Tests.SearchEngine.DelegateConstructors
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.Search;
    using LinqToDB.SqlQuery.Search.PathBuilder;
    using LinqToDB.SqlQuery.Search.TypeGraph;

    using Xunit;

    public class DelegateConstructorTest
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

            [SearchContainer]
            ID D { get; set; }
        }

        private interface IC : IBase
        {
            [SearchContainer]
            ID D { get; set; }
        }

        private interface ID : IBase
        {
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

            [SearchContainer]
            public ID D { get; set; }

            public B()
            {
                C = new C();
                D = new D();
            }
        }

        private class C : IC
        {
            [SearchContainer]
            public ID D { get; set; }

            public C()
            {
                D = new D();
            }
        }

        private class D : ID
        {
        }

        [Fact]
        public void SimpleDelegateB()
        {
            var obj = new A();
            var deleg = SetupTest<IBase, IB>(obj);

            var result = new LinkedList<IB>();
            deleg(obj, result);

            Assert.Equal(1, result.Count);
            Assert.Equal(obj.B, result.First.Value);
        }

        [Fact]
        public void SimpleDelegateC()
        {
            var obj = new A();
            var deleg = SetupTest<IBase, IC>(obj);

            var result = new LinkedList<IC>();
            deleg(obj, result);

            Assert.Equal(1, result.Count);
            Assert.Equal(obj.B.C, result.First.Value);
        }

        [Fact]
        public void BranchDelegateD()
        {
            var obj = new A();
            var deleg = SetupTest<IBase, ID>(obj);

            var result = new LinkedList<ID>();
            deleg(obj, result);

            Assert.Equal(2, result.Count);
            //Assert.Equal(obj.B.C, result.First.Value);
        }

        private DelegateConstructor<TSearch>.ResultDelegate SetupTest<TBase, TSearch>(TBase obj) where TSearch : class
        {
            var typeGraph = new TypeGraph<TBase>(GetType().Assembly.GetTypes());

            var pathBuilder = new PathBuilder<TBase>(typeGraph);

            var paths = pathBuilder.Find(obj, typeof(TSearch));

            var delegateConstructor = new DelegateConstructor<TSearch>();
            return delegateConstructor.CreateResultDelegate(paths);
        }
    }
}