using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bars2Db.SqlQuery.Search
{
    public static class SearchHelper<TBaseInterface>
    {
        private static readonly Dictionary<Type, List<Type>> baseInterfaces = new Dictionary<Type, List<Type>>();

        private static readonly Dictionary<Type, List<Type>> derivedInterfaces = new Dictionary<Type, List<Type>>();

        private static readonly Dictionary<Type, List<Type>> leafInterfaces = new Dictionary<Type, List<Type>>();

        static SearchHelper()
        {
            var baseType = typeof(TBaseInterface);
            var allTypes = baseType.Assembly.GetTypes().Where(type => baseType.IsAssignableFrom(type)).ToList();

            if (
                allTypes.Any(
                    t =>
                        t.IsGenericType &&
                        t.GetProperties().Any(p => p.GetCustomAttribute<SearchContainerAttribute>() != null)))
            {
                throw new NotSupportedException("SearchContainerAttribute in generic class");
            }

            allTypes = allTypes.Where(t => !t.IsGenericType).ToList();

            var allInterfaces = allTypes.Where(t => t.IsInterface).ToList();
            var allClasses = allTypes.Where(t => !t.IsInterface).ToList();

            foreach (var type in allTypes)
            {
                var interfaces =
                    type.GetInterfaces().Where(i => baseType.IsAssignableFrom(i) && !i.IsGenericType).ToList();

                baseInterfaces[type] = interfaces;

                derivedInterfaces[type] = allInterfaces.Where(t => type.IsAssignableFrom(t) && type != t).ToList();
            }

            foreach (var type in allClasses)
            {
                var interfaces = baseInterfaces[type];
                leafInterfaces[type] =
                    interfaces.Where(leaf => !interfaces.Any(i => leaf.IsAssignableFrom(i) && leaf != i)).ToList();
            }
        }

        public static IEnumerable<Type> GetAllSearchInterfaces(IEnumerable<Type> types)
        {
            var baseType = typeof(TBaseInterface);
            var inter =
                types.Where(t => baseType.IsAssignableFrom(t) && t.IsInterface && !t.IsGenericType)
                    .SelectMany(FindBaseWithSelf)
                    .Distinct()
                    .ToList();
            var interfaces =
                inter.SelectMany(t => t.GetProperties())
                    .Where(p => p.GetCustomAttribute<SearchContainerAttribute>() != null)
                    .Select(p => p.DeclaringType)
                    .Concat(inter)
                    .Distinct();

            return interfaces;
        }

        public static IEnumerable<Type> FindLeafInterfaces(Type classType)
        {
            if (!leafInterfaces.ContainsKey(classType))
            {
                throw new ArgumentOutOfRangeException();
            }

            return leafInterfaces[classType];
        }

        public static IEnumerable<Type> FindBase(Type propertyType)
        {
            if (!baseInterfaces.ContainsKey(propertyType))
            {
                throw new ArgumentOutOfRangeException();
            }

            return baseInterfaces[propertyType];
        }

        public static IEnumerable<Type> FindDerived(Type propertyType)
        {
            if (!derivedInterfaces.ContainsKey(propertyType))
            {
                throw new ArgumentOutOfRangeException();
            }

            return derivedInterfaces[propertyType];
        }

        public static IEnumerable<Type> FindHierarchy(Type propertyType)
        {
            var interfaces = FindBase(propertyType);

            if (!propertyType.IsInterface)
            {
                return interfaces;
            }

            return interfaces.Concat(FindDerived(propertyType)).Concat(new[] {propertyType});
        }


        public static IEnumerable<Type> FindBaseWithSelf(Type propertyType)
        {
            var interfaces = FindBase(propertyType);

            return propertyType.IsInterface
                ? interfaces.Concat(new[] {propertyType})
                : interfaces;
        }
    }
}