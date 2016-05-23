using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bars2Db.Linq.Joiner.Interfaces
{
    /// <summary>Сервиса для джойна сущностей на одном уровне</summary>
    public interface IJoinService
    {
        /// <summary>Джойн данных из <paramref name="sourceData" /> с данными из <paramref name="joinDataDictionary" />
        /// </summary>
        /// <param name="sourceData">Оригинальный запрос</param>
        /// <param name="joinDataDictionary">Данные для тех свойств которые необходимо приджойнить</param>
        /// <param name="allPropertiesInQuery">Все свойства текущего Jion учавствующие в запросе</param>
        /// <returns>Запрос в котором заполнеными полями</returns>
        IQueryable JoinData(IQueryable sourceData, IDictionary<PropertyInfo, IQueryable> joinDataDictionary, PropertyInfo[] allPropertiesInQuery);
    }
}