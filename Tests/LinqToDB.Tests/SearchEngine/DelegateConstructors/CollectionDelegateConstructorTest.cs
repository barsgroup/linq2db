namespace LinqToDB.Tests.SearchEngine.DelegateConstructors
{
    using System.Collections.Generic;
    using System.Linq;

    using LinqToDB.SqlQuery.Search;
    using LinqToDB.SqlQuery.Search.PathBuilder;
    using LinqToDB.SqlQuery.Search.TypeGraph;

    using Xunit;

    public class CollectionDelegateConstructorTest
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
            List<IC> C { get; set; }
        }

        private interface IC : IBase
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
            public List<IC> C { get; set; }

            public B()
            {
                C = new List<IC>
                    {
                    };
            }
        }

        private class C : IC
        {
            public IA A { get; set; }
        }

        [Fact]
        public void EmptyCollectionDelegate()
        {
            LinkedList<IC> result = new LinkedList<IC>();

            IA obj = new A();

            var deleg = SetupTest<IBase, IC>(obj);
            deleg(obj, result);

            var resultArray = result.ToArray();

            Assert.Equal(0, resultArray.Length);
        }

        [Fact]
        public void SimpleCollectionDelegate()
        {
            LinkedList<IC> result = new LinkedList<IC>();

            IA obj = new A();
            obj.B.C.Add(new C());
            obj.B.C.Add(new C());
            obj.B.C.Add(new C());

            var deleg = SetupTest<IBase, IC>(obj);
            deleg(obj, result);

            var resultArray = result.ToArray();

            Assert.Equal(3, resultArray.Length);

            var c = obj.B.C;
            for (var i = 0; i < resultArray.Length; i++)
            {
                Assert.Equal(c[i], resultArray[i]);
            }
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