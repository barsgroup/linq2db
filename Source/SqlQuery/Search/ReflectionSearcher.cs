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

            FindInternal(source, result, new HashSet<object>(), stepIntoFound);

            return result;
        }

        private static void FindInternal<TSearch>(object obj, LinkedList<TSearch> result, HashSet<object> visited, bool stepIntoFound) where TSearch : class
        {
            if (obj == null)
            {
                return;
            }

            if (visited.Contains(obj))
            {
                return;
            }

            visited.Add(obj);

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
                    foreach (var elem in dictionary.Values)
                    {
                        HandleValue(elem, result, visited, stepIntoFound);
                    }

                    return;
                }

                var collection = value as IEnumerable;
                if (collection != null)
                {
                    foreach (var elem in collection)
                    {
                        HandleValue(elem, result, visited, stepIntoFound);
                    }

                    return;
                }

                HandleValue(value, result, visited, stepIntoFound);
            }
        }

        private static void HandleValue<TSearch>(object value, LinkedList<TSearch> result, HashSet<object> visited, bool stepIntoFound) where TSearch : class
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

            FindInternal(value, result, visited, stepIntoFound);
        }
    }
}
