namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class SearchExtensions
    {
        public static IEnumerable<Type> FindInterfaces<TBaseInterface>(this Type propertyType)
        {
            return propertyType.GetInterfaces().Where(typeof(TBaseInterface).IsAssignableFrom);
        }

        public static IEnumerable<Type> FindInterfacesWithSelf<TBaseInterface>(this Type propertyType)
        {
            var interfaces = FindInterfaces<TBaseInterface>(propertyType);

            return propertyType.IsInterface
                       ? interfaces.Concat(new[] { propertyType })
                       : interfaces;
        }
    }
}
