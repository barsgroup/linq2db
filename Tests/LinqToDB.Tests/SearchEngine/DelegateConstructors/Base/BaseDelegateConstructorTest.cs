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
            deleg(testObj, resultStepInto, true);

            var resultNoStepInto = new LinkedList<TSearch>();
            deleg(testObj, resultNoStepInto, false);

            //// ---

            var expectedStepInto = ReflectionSearcher.Find<TSearch>(testObj, true);
            var expectedNoStepInto = ReflectionSearcher.Find<TSearch>(testObj, false);

            return IsEqual(resultStepInto, expectedStepInto) && IsEqual(expectedNoStepInto, resultNoStepInto);
        }

        private bool IsEqual<TSearch>(LinkedList<TSearch> list1, LinkedList<TSearch> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }

            var dic1 = PrepareResultCounter(list1);
            var dic2 = PrepareResultCounter(list2);

            if (dic1.Count != dic2.Count)
            {
                return false;
            }

            foreach (var elem in dic1)
            {
                int count2;
                if (!dic2.TryGetValue(elem.Key, out count2))
                {
                    return false;
                }

                if (elem.Value != count2)
                {
                    return false;
                }
            }

            return true;
        }

        private Dictionary<TSearch, int> PrepareResultCounter<TSearch>(LinkedList<TSearch> list)
        {
            var dictionary = new Dictionary<TSearch, int>();
            list.ForEach(
                node =>
                    {
                        if (!dictionary.ContainsKey(node.Value))
                        {
                            dictionary[node.Value] = 1;
                        }
                        else
                        {
                            dictionary[node.Value] += 1;
                        }
                    });

            return dictionary;
        }
    }
}
