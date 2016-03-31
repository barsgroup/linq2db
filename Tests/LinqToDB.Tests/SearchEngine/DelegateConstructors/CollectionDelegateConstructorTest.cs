namespace LinqToDB.Tests.SearchEngine.DelegateConstructors
{
    using System.Collections.Generic;
    using System.Linq;

    using LinqToDB.SqlQuery.Search;
    using LinqToDB.SqlQuery.Search.PathBuilder;
    using LinqToDB.SqlQuery.Search.TypeGraph;
    using LinqToDB.Tests.SearchEngine.DelegateConstructors.Base;

    using Xunit;

    public class CollectionDelegateConstructorTest : BaseDelegateConstructorTest<CollectionDelegateConstructorTest.IBase>
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
            List<IC> C { get; set; }
        }

        private interface IC : IBase
        {
            [SearchContainer]
            Dictionary<int, ID> D { get; set; }
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
            public List<IC> C { get; set; }

            public IC C1 { get; set; }

            public B()
            {
                C = new List<IC>
                    {
                    };

                C1 = new C();
            }
        }

        private class C : IC
        {
            public Dictionary<int, ID> D { get; set; }

            public C()
            {
                D = new Dictionary<int, ID>();
            }
        }

        private class D : ID
        {
        }

        [Fact]
        public void EmptyCollectionDelegate()
        {
            LinkedList<IC> result = new LinkedList<IC>();

            IA obj = new A();

            var deleg = SetupTest<IBase, IC>(obj);
            deleg(obj, result, true);

            var resultArray = result.ToArray();

            Assert.Equal(0, resultArray.Length);
            Assert.True(CompareWithReflectionSearcher<IB>(obj));
            Assert.True(CompareWithReflectionSearcher<IC>(obj));
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
            deleg(obj, result, true);

            var resultArray = result.ToArray();

            Assert.Equal(3, resultArray.Length);

            var c = obj.B.C;
            for (var i = 0; i < resultArray.Length; i++)
            {
                Assert.Equal(c[i], resultArray[i]);
            }

            Assert.True(CompareWithReflectionSearcher<IB>(obj));
            Assert.True(CompareWithReflectionSearcher<IC>(obj));
        }

        [Fact]
        public void MultipleCollectionDelegate()
        {
            var result = new LinkedList<ID>();

            var d11 = new D();
            var d12 = new D();
            var d13 = new D();
            var d21 = new D();

            var c1 = new C();
            c1.D[1] = d11;
            c1.D[2] = d12;
            c1.D[3] = d13;

            var c2 = new C();
            c2.D[1] = d21;

            var c3 = new C();

            var c4 = new C();
            c4.D = null;

            var b = new B();
            b.C.Add(c1);
            b.C.Add(c2);
            b.C.Add(c3);
            b.C.Add(c4);
            
            IA obj = new A();
            obj.B = b;

            var deleg = SetupTest<IBase, ID>(obj);
            deleg(obj, result, true);

            var resultArray = result.ToArray();

            Assert.Equal(4, resultArray.Length);
            
            Assert.True(CompareWithReflectionSearcher<IB>(obj));
            Assert.True(CompareWithReflectionSearcher<IC>(obj));
            Assert.True(CompareWithReflectionSearcher<ID>(obj));

        }

        private ResultDelegate<TSearch> SetupTest<TBase, TSearch>(TBase obj) where TSearch : class
        {
            var typeGraph = new TypeGraph<TBase>(GetType().Assembly.GetTypes());

            var pathBuilder = new PathBuilder<TBase>(typeGraph);

            var paths = pathBuilder.Find(obj, typeof(TSearch));

            var delegateConstructor = new DelegateConstructor<TSearch>();
            return delegateConstructor.CreateResultDelegate(paths);
        }
    }
}