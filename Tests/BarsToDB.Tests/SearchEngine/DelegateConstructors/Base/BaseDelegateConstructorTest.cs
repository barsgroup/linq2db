namespace LinqToDB.Tests.SearchEngine.DelegateConstructors.Base
{
    using System.Collections.Generic;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.Search;

    using SqlQuery.Search.PathBuilder;
    using SqlQuery.Search.TypeGraph;

    public abstract class BaseDelegateConstructorTest<TBase>
    {
        public bool CompareWithReflectionSearcher<TSearch>(TBase testObj) where TSearch : class
        {
            var typeGraph = new TypeGraph<TBase>(GetType().Assembly.GetTypes());
            var pathBuilder = new PathBuilder<TBase>(typeGraph);

            var delegateConstructor = new DelegateConstructor<TSearch>();

            var paths = pathBuilder.Find<TSearch>(testObj);
            var deleg = delegateConstructor.CreateResultDelegate(paths);

            //// ---

            var resultStepInto = new LinkedList<TSearch>();
            deleg(testObj, resultStepInto, true, new HashSet<object>());

            var resultNoStepInto = new LinkedList<TSearch>();
            deleg(testObj, resultNoStepInto, false, new HashSet<object>());

            //// ---

            var stepIntoIsEqual = ReflectionSearcher.FindAndCompare(testObj, true, resultStepInto);
            var noStepIntoIsEqual = ReflectionSearcher.FindAndCompare(testObj, false, resultNoStepInto);

            return stepIntoIsEqual && noStepIntoIsEqual;
        }
    }
}
