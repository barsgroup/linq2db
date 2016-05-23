using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bars2Db.Linq.Joiner.Interfaces;
using Bars2Db.Linq.Joiner.PropertyJoiner;

namespace Bars2Db.Linq.Joiner
{
    /// <summary>Сервиса для джойна сущностей на одном уровне</summary>
    public class JoinService : IJoinService
    {
        private class NsiMetaIdJoinDelegateMetaInfo
        {
            public Delegate Delegate { get; set; }

            public PropertyInfo[] DelegateArgumentsPropertyInfos { get; set; }
        }

        //private readonly ConcurrentDictionary<Type, ConcurrentDictionary<HashSet<PropertyInfo>, NsiMetaIdJoinDelegateMetaInfo>> _cache =
        //    new ConcurrentDictionary<Type, ConcurrentDictionary<HashSet<PropertyInfo>, NsiMetaIdJoinDelegateMetaInfo>>();

        /// <summary>Джойн данных из <paramref name="sourceData" /> с данными из <paramref name="joinDataDictionary" />
        /// </summary>
        /// <param name="sourceData">Оригинальный запрос</param>
        /// <param name="joinDataDictionary">Данные для тех свойств которые необходимо приджойнить</param>
        /// <param name="allPropertiesInQuery">Все свойства текущего Jion учавствующие в запросе</param>
        /// <returns>Запрос в котором заполнеными полями</returns>
        public IQueryable JoinData(IQueryable sourceData, IDictionary<PropertyInfo, IQueryable> joinDataDictionary, PropertyInfo[] allPropertiesInQuery)
        {
            var properties = new HashSet<PropertyInfo>(joinDataDictionary.Select(x => x.Key));

            if (properties.Count == 0)
            {
                return sourceData;
            }

            var delegateInfo = GetJoinDelegate(sourceData.ElementType, allPropertiesInQuery);

            IEnumerable<object> arguments = new object[] { sourceData };

            var queries = delegateInfo.DelegateArgumentsPropertyInfos.Select(propertyInfo => joinDataDictionary.First(q => q.Key.Name == propertyInfo.Name).Value);

            arguments = arguments.Concat(queries);

            var joinResult = (IQueryable)delegateInfo.Delegate.DynamicInvoke(arguments.ToArray());

            return joinResult;
        }

        /// <summary>Получить делегат для сущности типа T</summary>
        /// <param name="type">Тип исходной сущности к которой будет осуществляться join</param>
        /// <param name="allPropertiesInQuery">Все свойства текущего Jion учавствующие в запросе</param>
        private NsiMetaIdJoinDelegateMetaInfo GetJoinDelegate(Type type, PropertyInfo[] allPropertiesInQuery)
        {
            //ConcurrentDictionary<HashSet<PropertyInfo>, NsiMetaIdJoinDelegateMetaInfo> resultDictionary;

            //if (_cache.TryGetValue(type, out resultDictionary))
            //{
            //    HashSet<PropertyInfo> findedPropertyInfosSet;
            //    if (TryGetPropertyInfoCollection(resultDictionary.Keys, propertiesToJoin, out findedPropertyInfosSet) &&
            //        resultDictionary.TryGetValue(findedPropertyInfosSet, out result))
            //    {
            //        return result;
            //    }
            //}
            //else
            //{
            //    resultDictionary = new ConcurrentDictionary<HashSet<PropertyInfo>, NsiMetaIdJoinDelegateMetaInfo>();
            //}

            var creator = new JoinByLinkFieldQueryCreator();
            PropertyInfo[] propertyInfos;
            var joinDelegate = creator.CreateJoinDelegate(type, out propertyInfos, allPropertiesInQuery);

            var result = new NsiMetaIdJoinDelegateMetaInfo
                                                       {
                                                           Delegate = joinDelegate,
                                                           DelegateArgumentsPropertyInfos = propertyInfos.ToArray()
                                                       };

            //resultDictionary.TryAdd(propertiesToJoin, result);

            //_cache.TryAdd(type, resultDictionary);

            return result;
        }

        /// <summary>
        ///     Поиск закэшированной коллекции свойств, которая совпадает по содержимому с переданным списком подлежащих
        ///     приджойниванию свойств
        /// </summary>
        /// <param name="cashedPropertyInfosCollection">Коллекция закэшированных наборов приджойненных своств</param>
        /// <param name="seekingCollection">Искомый набор свойств</param>
        /// <param name="propertyInfoCollection">Найденный набор свойств</param>
        private bool TryGetPropertyInfoCollection(ICollection<HashSet<PropertyInfo>> cashedPropertyInfosCollection,
                                                  HashSet<PropertyInfo> seekingCollection,
                                                  out HashSet<PropertyInfo> propertyInfoCollection)
        {
            propertyInfoCollection = cashedPropertyInfosCollection.SingleOrDefault(propertyInfos => propertyInfos.SetEquals(seekingCollection));

            return propertyInfoCollection != null;
        }
    }
}