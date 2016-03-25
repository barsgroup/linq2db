namespace LinqToDB.Tests.SearchEngine.DelegateConstructors
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.Search;

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
        }

        [Fact]
        public void SimpleDelegate()
        {
            var typeGraph = new TypeGraph<IBase>(GetType().Assembly.GetTypes());

            var pathBuilder = new PathBuilder<IBase>(typeGraph);

            var obj = new A();
            var paths = pathBuilder.Find(obj, typeof(IB));

            var delegateConstructor = new DelegateConstructor<IB>();
            var deleg = delegateConstructor.CreateResultDelegate(paths);

            LinkedList<IB> result = new LinkedList<IB>();
            deleg(obj, out result);
        }
    }
}