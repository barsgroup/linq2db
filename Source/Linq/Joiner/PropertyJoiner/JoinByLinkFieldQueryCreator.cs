using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bars.Minfin.Kaliningrad.Services.PropertiesJoiner.Entities;
using Bars2Db.Extensions;
using Bars2Db.Linq.Joiner.PropertyJoiner.Attributes;

namespace Bars2Db.Linq.Joiner.PropertyJoiner
{
    /// <summary>
    ///     Класс для создания делегатов для запроса данных из БД с учетом Join по связующему полю (Например: MetaId или
    ///     Id)
    /// </summary>
    public class JoinByLinkFieldQueryCreator
    {
        /// <summary>Формирует делегат для запроса данных из БД с учетом Join по связующему полю</summary>
        /// <param name="sourceEntityType">Тип сущности к которой будет происходит джойн</param>
        /// <param name="joinDelegateParametersTypes">
        ///     Массив типов сущностей, данные которых необходимо передать в результирующий
        ///     делегат для джоина.
        /// </param>
        /// <param name="allPropertiesInQuery">Все свойства текущего Jion учавствующие в запросе</param>
        /// <returns>Формирует делегат для запроса данных из БД с учетом Join по связующему полю</returns>
        public Delegate CreateJoinDelegate(Type sourceEntityType, out PropertyInfo[] joinDelegateParametersTypes, PropertyInfo[] allPropertiesInQuery)
        {
            var joinDescriptors = GetPropertiesToJoin(sourceEntityType, allPropertiesInQuery).ToList();

            var allOtherProperties = GetAllOtherProperies(sourceEntityType);

            // Определяем параметр для результирующего Lambda выражения
            var originalDataParameter = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(sourceEntityType), sourceEntityType.Name.ToLowerInvariant());

            // Список параметров, которые в дальнейшем будут использоваться для обращения к получившемуся Expression'у
            var propertyInfoToQueryParameter = new List<KeyValuePair<PropertyInfo, ParameterExpression>>();
            var alreadyJoinedProperties = new List<PropertyInfo>(joinDescriptors.Count);

            // В левой части после каждой итерации будет значение предыдущего Join'а в виде Join().Select()
            Expression leftQuery = originalDataParameter;

            // Идем по всем полученным дескрипторам, чтобы осуществить Join всех связанных полей
            for (var i = 0; i < joinDescriptors.Count; i++)
            {
                // Текущий дескриптор
                var joinDescriptor = joinDescriptors[i];

                var leftPropertyInfo = joinDescriptor.LeftJoinProperty;
                var rightPropertyInfo = joinDescriptor.RightJoinProperty;

                // Определяем параметр для правой части Join'а
                var rightType = joinDescriptor.LeftEntityProperty.PropertyType;
                var rightQuery = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(rightType), rightType.Name.ToLowerInvariant());

                // Запоминаем получивший параметр, чтобы потом не забыть его при вызове получившегося выражения
                propertyInfoToQueryParameter.Add(new KeyValuePair<PropertyInfo, ParameterExpression>(joinDescriptor.LeftEntityProperty, rightQuery));

                var leftParameter = Expression.Parameter(sourceEntityType, "left");
                Expression leftPropertyExpression = Expression.Property(leftParameter, leftPropertyInfo);

                var rightParameter = Expression.Parameter(rightType, "right");
                Expression rightPropertyExpression = Expression.Property(rightParameter, rightPropertyInfo);

                //приведение типа на случай, если левое поле Nullable
                if (leftPropertyInfo.PropertyType.IsNullable())
                {
                    rightPropertyExpression = Expression.Convert(rightPropertyExpression, leftPropertyInfo.PropertyType);
                }

                var leftPropertyLambda = Expression.Lambda(leftPropertyExpression, leftParameter);
                var rightPropertyLambda = Expression.Lambda(rightPropertyExpression, rightParameter);

                var joinType = leftPropertyInfo.PropertyType.IsNullable()
                                   ? leftPropertyInfo.PropertyType
                                   : rightPropertyInfo.PropertyType;

                // Вызываем метод из QueryProviderExtensions, в котором описана логика Join'а, для результата предыдущей итерации и
                var joinExpression = Expression.Call(
                    typeof(QueryProviderExtensions),
                    GetJoinExtensionMethodInfo(sourceEntityType, rightType).Name,
                    new[] { sourceEntityType, rightType, joinType },
                    leftQuery,
                    rightQuery,
                    Expression.Quote(leftPropertyLambda),
                    Expression.Quote(rightPropertyLambda));

                // Получаем тип результата Join'а, чтобы передать его в построитель Lambd'ы
                var resultType = typeof(JoinResultObject<,>).MakeGenericType(sourceEntityType, rightType);
                var selectLambda = CreateSelectExpression(sourceEntityType, resultType, joinDescriptor, alreadyJoinedProperties, allOtherProperties);

                // Строим результирующий Select и помещаем его в leftQuery, чтобы на следующей итерации
                // воспользоваться этим результатом в качестве первого параметра Join'а
                leftQuery = Expression.Call(typeof(Queryable), "Select", new[] { resultType, sourceEntityType }, joinExpression, selectLambda);
            }

            var resultExpression = leftQuery as MethodCallExpression;
            var queryableParameters = new[] { originalDataParameter }.Concat(propertyInfoToQueryParameter.Select(x => x.Value));
            var resultDelegate = Expression.Lambda(resultExpression, queryableParameters).Compile();

            //1 пропускается так как это параметр оригинальных данных и требуется по-умолчанию
            joinDelegateParametersTypes = propertyInfoToQueryParameter.Select(x => x.Key).ToArray();

            return resultDelegate;
        }

        /// <summary>Создать Expression для Select результатов Join'а</summary>
        /// <param name="sourceEntityType">Тип сущности к которой будет происходит джойн</param>
        /// <param name="resultType">результирующий тип после всех джойнов</param>
        /// <param name="joinProperyDescriptor">PropertyInfo того свойство, которое сейчас Join'ится</param>
        /// <param name="alreadyJoinedProperties">Список свойств, которые уже Join'или на предыдущих итерациях</param>
        /// <param name="allOtherProperties">Все свойства текущего Jion учавствующие в запросе, кроме приджойненных</param>
        private LambdaExpression CreateSelectExpression(Type sourceEntityType,
                                                        Type resultType,
                                                        PropertyJoinDescriptor joinProperyDescriptor,
                                                        List<PropertyInfo> alreadyJoinedProperties,
                                                        HashSet<PropertyInfo> allOtherProperties)
        {
            var bindList = new List<MemberBinding>();

            var resultParameter = Expression.Parameter(resultType, "Result");

            // Заполняем ранее приджойненные свойства, так как строится новая лямбда
            Expression leftParameterExpression = Expression.Property(resultParameter, "Left");
            foreach (var alreadyJoinedProperty in alreadyJoinedProperties)
            {
                bindList.Add(Expression.Bind(alreadyJoinedProperty, Expression.Property(leftParameterExpression, alreadyJoinedProperty.Name)));
            }

            Expression resultMemberAccessor = resultParameter;

            // Строим Bind для свойство, которое Join'им на этой итерации
            var propertyInfo = joinProperyDescriptor.LeftEntityProperty;
            bindList.Add(Expression.Bind(propertyInfo, Expression.Property(resultMemberAccessor, "Right")));

            // Не забываем добавить свойство, которое Join'или на этой итерации в список, чтобы на следующей про него не забыть
            alreadyJoinedProperties.Add(propertyInfo);

            // Теперь добавим остальные свойства в лямбду (кроме тех, которые могут джойниться)
            resultMemberAccessor = Expression.Property(resultMemberAccessor, "Left");

            bindList.AddRange(allOtherProperties.Select(property => Expression.Bind(property, Expression.Property(resultMemberAccessor, property.Name))));

            // Собственно собираем готовую лямбду из списка свойств, которые сформировали ранее
            var selectLambda = Expression.Lambda(Expression.MemberInit(Expression.New(sourceEntityType), bindList), resultParameter);
            return selectLambda;
        }

        private HashSet<PropertyInfo> GetAllOtherProperies(Type sourceType)
        {
            //TODO: Исправить
            var otherProperties =
                new HashSet<PropertyInfo>(
                    sourceType.GetProperties()
                              .Where(p => !p.GetCustomAttributes().OfType<BaseJoinPropertyAttribute>().Any()));// && !p.GetCustomAttributes().OfType<DbColumnIgnoreAttribute>().Any()));

            return otherProperties;
        }

        /// <summary>Получить ссылку на метод, который выполняет Join для указанных сущностей</summary>
        /// <param name="lefType">Тип сущности, к которой происходит Join</param>
        /// <param name="righType">Тип сущности, которая Join'ится</param>
        private MethodInfo GetJoinExtensionMethodInfo(Type lefType, Type righType)
        {
            Func<string, MethodInfo> getMethodInfoByNameFunc = s => typeof(QueryProviderExtensions).GetMethod(s);

            MethodInfo methodInfo = getMethodInfoByNameFunc("DefaultJoin");

            if (methodInfo == null)
            {
                throw new NullReferenceException("Не удалось получить метод для выполнения Join'а между сущностями");
            }

            return methodInfo;
        }

        /// <summary>Получить коллекцию описателей всех джойнов, которые будут происходить</summary>
        private IEnumerable<PropertyJoinDescriptor> GetPropertiesToJoin(Type sourceEntityType, IEnumerable<PropertyInfo> propertiesToJoin)
        {
            var allProperties = sourceEntityType.GetProperties();

            var propertiesToJoinNames = propertiesToJoin.Select(x => x.Name).ToArray();

            //TODO Исправить
            var entityProperties = allProperties.ToList();//.Where(p => p.PropertyType.Is<IEntity>()).ToList();

            return from propertyInfo in entityProperties
                   let joinAttribute = propertyInfo.GetCustomAttributes().OfType<BaseJoinPropertyAttribute>().SingleOrDefault()
                   where joinAttribute != null && propertiesToJoinNames.Contains(propertyInfo.Name)
                   select new PropertyJoinDescriptor
                          {
                              LeftEntityProperty = propertyInfo,
                              LeftJoinProperty = allProperties.Single(p => p.Name == joinAttribute.GetOurKeyPropertyName(propertyInfo)),
                              RightJoinProperty = propertyInfo.PropertyType.GetProperty(joinAttribute.JoinOnPropertyName)
                          };
        }

        private class PropertyJoinDescriptor
        {
            /// <summary>Свойство по которому будет проходить джойн в сущности которая будет присоединена</summary>
            public PropertyInfo RightJoinProperty { get; set; }

            /// <summary>Свойство со ссылкой на присоединяемую сущность</summary>
            public PropertyInfo LeftEntityProperty { get; set; }

            /// <summary>Свойство по которому будет проходить джойн в присоединяющей сущности</summary>
            public PropertyInfo LeftJoinProperty { get; set; }
        }
    }
}