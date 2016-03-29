namespace LinqToDB.SqlQuery.Search
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class ReflectionSearcher
    {
        public static LinkedList<TSearch> Find<TSearch>(object source, bool stepIntoFound) where TSearch : class
        {
            var result = new LinkedList<TSearch>();

            FindInternal(source, result, stepIntoFound);

            return result;
        }

        private static void FindInternal<TSearch>(object obj, LinkedList<TSearch> result, bool stepIntoFound) where TSearch : class
        {
            if (obj == null)
            {
                return;
            }

            var properties = obj.GetType().GetInterfaces().SelectMany(i => i.GetProperties().Where(p => p.GetCustomAttribute<SearchContainerAttribute>() != null));

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);

                if (value == null)
                {
                    continue;
                }

                var dictionary = value as IDictionary;
                if (dictionary != null)
                {
                    HandleCollection(dictionary.Values, result, stepIntoFound);
                    return;
                }

                var collection = value as IEnumerable;
                if (collection != null)
                {
                    HandleCollection(collection, result, stepIntoFound);
                    return;
                }

                HandleValue(value, result, stepIntoFound);
            }
        }

        private static void HandleCollection<TSearch>(IEnumerable collection, LinkedList<TSearch> result, bool stepIntoFound) where TSearch : class
        {
            foreach (var elem in collection)
            {
                HandleValue(elem, result, stepIntoFound);
            }
        }

        private static void HandleValue<TSearch>(object value, LinkedList<TSearch> result, bool stepIntoFound) where TSearch : class
        {
            var searchValue = value as TSearch;

            if (searchValue != null)
            {
                result.AddLast(searchValue);

                if (!stepIntoFound)
                {
                    return;
                }
            }

            FindInternal(value, result, stepIntoFound);
        }
    }
}
