namespace LinqToDB.SqlQuery.Search
{
    using System;

    public class SearchEngine<TBaseSearchInterface>
    {
        private readonly PathBuilder<TBaseSearchInterface> _pathBuilder;

        private SearchEngine()
        {
            var types = typeof(TypeVertex).Assembly.GetTypes();
            var typeGraph = new TypeGraph<TBaseSearchInterface>(types);
            _pathBuilder = new PathBuilder<TBaseSearchInterface>(typeGraph);
        }

        public static SearchEngine<TBaseSearchInterface> Current { get; } = new SearchEngine<TBaseSearchInterface>();

        public void Find<TElement>(TBaseSearchInterface source)
        {
            Find(source, typeof(TElement));
        }

        public void Find(TBaseSearchInterface source, Type searchType)
        {
            _pathBuilder.Find(source, searchType);
        }
    }

    //private Type GetSuitableInterface(Type type)
        //{
        //    var list = type.GetInterfaces().Where(typeof(TBaseSearchInterface).IsAssignableFrom).ToList();

        //    Type resultType = null;
        //    for (var i = 0; i < list.Count; i++)
        //    {
        //        var j = 0;
        //        while (i == j || j < list.Count && !list[i].IsAssignableFrom(list[j]))
        //        {
        //            j++;
        //        }

        //        if (j == list.Count)
        //        {
        //            if (resultType == null)
        //            {
        //                resultType = list[i];
        //            }
        //            else
        //            {
        //                throw new InvalidOperationException("Обнаружено несколько рутовых интерфейсов");
        //            }
        //        }
        //    }

        //    if (resultType != null)
        //    {
        //        return resultType;
        //    }

        //    throw new InvalidOperationException("Все типы зависят друг от друга");
        //}


       // public static PropertyInfo[] GetPublicProperties(Type type)
       // {
       //     if (type.IsInterface)
       //     {
       //         var propertyInfos = new List<PropertyInfo>();
       //
       //         var considered = new List<Type>();
       //         var queue = new Queue<Type>();
       //         considered.Add(type);
       //         queue.Enqueue(type);
       //         while (queue.Count > 0)
       //         {
       //             var subType = queue.Dequeue();
       //             foreach (var subInterface in subType.GetInterfaces())
       //             {
       //                 if (considered.Contains(subInterface)) continue;
       //
       //                 considered.Add(subInterface);
       //                 queue.Enqueue(subInterface);
       //             }
       //
       //             var typeProperties = subType.GetProperties(
       //                 BindingFlags.FlattenHierarchy
       //                 | BindingFlags.Public
       //                 | BindingFlags.Instance);
       //
       //             var newPropertyInfos = typeProperties
       //                 .Where(x => !propertyInfos.Contains(x));
       //
       //             propertyInfos.InsertRange(0, newPropertyInfos);
       //         }
       //
       //         return propertyInfos.ToArray();
       //     }
       //
       //     return type.GetProperties(BindingFlags.FlattenHierarchy
       //         | BindingFlags.Public | BindingFlags.Instance);
       // }
}