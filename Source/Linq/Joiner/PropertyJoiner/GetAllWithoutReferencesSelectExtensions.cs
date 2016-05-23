using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bars2Db.Linq.Joiner.PropertyJoiner.Attributes;

namespace Bars2Db.Linq.Joiner.PropertyJoiner
{
    /// <summary>Extension для обхода ошибки при вызове GetAllWithoutReferences без подтягивания ссылок</summary>
    public static class GetAllWithoutReferencesSelectExtensions
    {
        private static readonly ConcurrentDictionary<Type, Expression> Cache = new ConcurrentDictionary<Type, Expression>();

        /// <summary>Получить запрос для заполнения всех свойств, исключая те, что помечены атрибутом JoinPropertyAttribute
        ///     <para>Метод необходим для правильного заполнения свойств связанных сущностей при экспорте справочников</para>
        /// </summary>
        /// <param name="query">Исходный запрос</param>
        public static IQueryable FillFieldsForExport(this IQueryable query)
        {
            var type = query.ElementType;
            var selectLambda = GetSelectLambda(type, infos => infos.GetCustomAttribute<BaseJoinPropertyAttribute>(true) == null, false);

            var selectCallExpression = Expression.Call(typeof(Queryable), "Select", new[] { type, type }, query.Expression, Expression.Quote(selectLambda));
            return query.Provider.CreateQuery(selectCallExpression);
        }

        /// <summary>Построить Lambda выражение для Select'а</summary>
        /// <param name="type">Тип сущности, для которой создается выражение</param>
        /// <param name="filterPropertiesFunc">Делагат для фильтрации свойств</param>
        private static Expression BuildSelectLambda(Type type, Func<PropertyInfo, bool> filterPropertiesFunc)
        {
            var resultParameter = Expression.Parameter(type, type.Name);
            var properties = type.GetProperties();

            if (filterPropertiesFunc != null)
            {
                properties = properties.Where(filterPropertiesFunc).ToArray();
            }

            var bindList = properties.Select(propertyInfo => Expression.Bind(propertyInfo, Expression.Property(resultParameter, propertyInfo.Name)));

            var selectLambda = Expression.Lambda(Expression.MemberInit(Expression.New(type), bindList), resultParameter);
            return selectLambda;
        }

        /// <summary>
        ///     Получить Lambda выражение для Select'а
        ///     <para>
        ///         Метод сперва пытается взять выражение из КЭШ'а, если там нет выражения, то создаем выражение и добавляем в
        ///         КЭШ
        ///     </para>
        /// </summary>
        /// <param name="type">Тип сущности, для которой создается выражение</param>
        /// <param name="filterPropertiesFunc">Делагат для фильтрации свойств</param>
        /// <param name="needCache">Определяет необходимость кэширования</param>
        private static Expression GetSelectLambda(Type type, Func<PropertyInfo, bool> filterPropertiesFunc, bool needCache)
        {
            Expression selectLambda;
            if (needCache)
            {
                if (!Cache.TryGetValue(type, out selectLambda))
                {
                    selectLambda = BuildSelectLambda(type, filterPropertiesFunc);
                    Cache.TryAdd(type, selectLambda);
                }
            }
            else
            {
                selectLambda = BuildSelectLambda(type, filterPropertiesFunc);
            }

            return selectLambda;
        }
    }
}