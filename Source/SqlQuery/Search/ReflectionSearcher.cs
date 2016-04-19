namespace LinqToDB.SqlQuery.Search
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using LinqToDB.Extensions;

    public static class ReflectionSearcher
    {
        public static LinkedList<TSearch> Find<TSearch>(object source, bool stepIntoFound, HashSet<object> visited = null) where TSearch : class
        {
            var result = new LinkedList<TSearch>();

            if (visited == null)
            {
                visited = new HashSet<object>();
            }

            if (stepIntoFound)
            {
                FindInternal(source, result, visited);
            }
            else
            {
                FindDownToInternal(source, source, result, visited);
            }

            return result;
        }

        public static bool FindAndCompare<TSearch>(object source, bool stepIntoFound, LinkedList<TSearch> result) where TSearch : class
        {
            var reflectionResult = Find<TSearch>(source, stepIntoFound);

            return IsSearchResultEqual(result, reflectionResult);
        }

        private static void FindInternal<TSearch>(object obj, LinkedList<TSearch> result, HashSet<object> visited) where TSearch : class
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
                        FindInternal(elem, result, visited);
                    }

                    continue;
                }
                
                var collection = value as IEnumerable;
                if (collection != null)
                {
                    foreach (var elem in collection)
                    {
                        FindInternal(elem, result, visited);
                    }

                    continue;
                }
                
                FindInternal(value, result, visited);
            }
        }
        
        private static void FindDownToInternal<TSearch>(object root, object obj, LinkedList<TSearch> result, HashSet<object> visited) where TSearch : class
        {
            if (obj == null || visited.Contains(obj))
            {
                return;
            }

            visited.Add(obj);

            var searchObj = obj as TSearch;
            if (searchObj != null && !ReferenceEquals(obj, root))
            {
                result.AddLast(searchObj);
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
                    foreach (var elem in dictionary.Values)
                    {
                        FindDownToInternal(root, elem, result, visited);
                    }

                    continue;
                }

                var collection = value as IEnumerable;
                if (collection != null)
                {
                    foreach (var elem in collection)
                    {
                        FindDownToInternal(root, elem, result, visited);
                    }

                    continue;
                }

                FindDownToInternal(root, value, result, visited);
            }
        }

        public static bool IsSearchResultEqual<TSearch>(LinkedList<TSearch> list1, LinkedList<TSearch> list2)
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

        private static Dictionary<TSearch, int> PrepareResultCounter<TSearch>(LinkedList<TSearch> list)
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
