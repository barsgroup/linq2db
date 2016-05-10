//namespace LinqToDB.Tests.SearchEngine.DelegateConstructors
//{
//    using System.Collections.Generic;

//    using LinqToDB.SqlQuery.Search;
//    using LinqToDB.SqlQuery.Search.PathBuilder;
//    using LinqToDB.SqlQuery.Search.TypeGraph;
//    using LinqToDB.Tests.SearchEngine.DelegateConstructors.Base;

//    using Xunit;

//    public class DelegateConstructorTest : BaseDelegateConstructorTest<DelegateConstructorTest.IBase>
//    {
//        public interface IBase
//        {

//        }

//        private interface IA : IBase
//        {
//            [SearchContainer]
//            IB B { get; set; }

//            [SearchContainer]
//            IE E { get; set; }
//        }

//        private interface IB : IBase
//        {
//            [SearchContainer]
//            IC C { get; set; }

//            [SearchContainer]
//            ID D { get; set; }

//            [SearchContainer]
//            IE E { get; set; }
//        }

//        private interface IC : IBase
//        {
//            [SearchContainer]
//            ID D { get; set; }
//        }

//        private interface ID : IBase
//        {
//        }

//        private interface IE : IBase
//        {
//        }

//        private class A : IA
//        {
//            public IB B { get; set; }

//            public IE E { get; set; }

//            public A()
//            {
//                B = new B();
//                E = new E();
//            }
//        }

//        private class B : IB
//        {
//            public IC C { get; set; }

//            public ID D { get; set; }

//            public IE E { get; set; }

//            public B()
//            {
//                C = new C();
//                D = new D();
//                E = new E();
//            }
//        }

//        private class C : IC
//        {
//            [SearchContainer]
//            public ID D { get; set; }

//            public C()
//            {
//                D = new D();
//            }
//        }

//        private class D : ID
//        {
//        }

//        private class E : IE
//        {
//        }

//        [Fact]
//        public void SimpleDelegateB()
//        {
//            var obj = new A();
//            var deleg = SetupTest<IBase, IB>(obj);

//            var result = new LinkedList<IB>();
//            deleg(obj, result, true, new HashSet<object>());

//            Assert.Equal(1, result.Count);
//            Assert.Equal(obj.B, result.First.Value);

//            Assert.True(CompareWithReflectionSearcher<IB>(obj));
//        }

//        [Fact]
//        public void SimpleDelegateC()
//        {
//            var obj = new A();
//            var deleg = SetupTest<IBase, IC>(obj);

//            var result = new LinkedList<IC>();
//            deleg(obj, result, true, new HashSet<object>());

//            Assert.Equal(1, result.Count);
//            Assert.Equal(obj.B.C, result.First.Value);

//            Assert.True(CompareWithReflectionSearcher<IC>(obj));
//        }

//        [Fact]
//        public void BranchDelegateD()
//        {
//            var obj = new A();
//            var deleg = SetupTest<IBase, ID>(obj);

//            var result = new LinkedList<ID>();
//            deleg(obj, result, true, new HashSet<object>());

//            Assert.Equal(2, result.Count);

//            Assert.True(CompareWithReflectionSearcher<ID>(obj));
//        }

//        [Fact]
//        public void TwoPathsDelegateE()
//        {
//            var obj = new A();
//            var deleg = SetupTest<IBase, IE>(obj);

//            var result = new LinkedList<IE>();
//            deleg(obj, result, true, new HashSet<object>());

//            Assert.Equal(2, result.Count);

//            Assert.True(CompareWithReflectionSearcher<IE>(obj));
//        }

//        private ResultDelegate<TSearch> SetupTest<TBase, TSearch>(TBase obj) where TSearch : class
//        {
//            var typeGraph = new TypeGraph<TBase>(GetType().Assembly.GetTypes());

//            var pathBuilder = new PathBuilder<TBase>(typeGraph);

//            var paths = pathBuilder.Find(obj, typeof(TSearch));

//            var delegateConstructor = new DelegateConstructor<TSearch>();
//            return delegateConstructor.CreateResultDelegate(paths);
//        }
//    }
//}

