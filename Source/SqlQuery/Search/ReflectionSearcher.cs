namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class ReflectionSearcher
    {
        public static LinkedList<TSearch> Find<TSearch>(object source) where TSearch : class
        {
            var result = new LinkedList<TSearch>();

            FindInternal(source, result);

            return result;
        }

        private static void FindInternal<TSearch>(object obj, LinkedList<TSearch> result) where TSearch : class
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

                // TODO: dictionary?
                var collection = value as IEnumerable;
                if (collection != null)
                {
                    foreach (var elem in collection)
                    {
                        HandleValue(elem, result);
                    }
                }
                else
                {
                    HandleValue(value, result);
                }
            }
        }

        private static void HandleValue<TSearch>(object value, LinkedList<TSearch> result) where TSearch : class
        {
            var searchValue = value as TSearch;

            if (searchValue != null)
            {
                result.AddLast(searchValue);
            }

            FindInternal(value, result);
        }
    }
}
