namespace LinqToDB.SqlQuery.Search
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class ReflectionSearcher
    {
        public static LinkedList<TSearch> Find<TSearch>(object source, bool stepIntoFound, HashSet<object> visited = null) where TSearch : class
        {
            var result = new LinkedList<TSearch>();

            if (visited == null)
            {
                visited = new HashSet<object>();
            }

            FindInternal(source, result, stepIntoFound, visited);

            return result;
        }

        private static void FindInternal<TSearch>(object obj, LinkedList<TSearch> result, bool stepIntoFound, HashSet<object> visited) where TSearch : class
        {
            if (obj == null || visited.Contains(obj))
            {
                return;
            }

            visited.Add(obj);

            var searchObj = obj as TSearch;
            if (searchObj != null)
            {
                result.AddLast(searchObj);

                if (!stepIntoFound)
                {
                    return;
                }
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
                    foreach (var elem in dictionary.Values)
                    {
                        FindInternal(elem, result, stepIntoFound, visited);
                    }

                    continue;
                }
                
                var collection = value as IEnumerable;
                if (collection != null)
                {
                    foreach (var elem in collection)
                    {
                        FindInternal(elem, result, stepIntoFound, visited);
                    }

                    continue;
                }
                
                FindInternal(value, result, stepIntoFound, visited);
            }
        }
    }
}
