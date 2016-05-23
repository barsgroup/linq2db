using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bars.Minfin.Kaliningrad.Services.PropertiesJoiner.Entities;

namespace Bars2Db.Linq.Joiner.PropertyJoiner
{
    internal static class QueryProviderExtensions
    {
        /// <summary>Джойнер по умолчанию
        ///     <para>Не учитывает этап версии</para>
        /// </summary>
        /// <typeparam name="TLeft">Тип сущности в левой части</typeparam>
        /// <typeparam name="TRight">Тип сущности в правой части</typeparam>
        /// <typeparam name="TProperty">Тип свойства, по которому происходит Join</typeparam>
        /// <param name="leftData">Запрос с данными, к которым будет происходить Join</param>
        /// <param name="rightData">Запрос с данными, которые будут Join'иться</param>
        /// <param name="leftField">Выражение, которое определяет поле в левом запросе для Join'а</param>
        /// <param name="rightField">Выражение, которое определяет поле в правом запросе для Join'а</param>
        /// <returns>Коллекция после операции Join</returns>
        public static IQueryable<JoinResultObject<TLeft, TRight>> DefaultJoin<TLeft, TRight, TProperty>(this IQueryable<TLeft> leftData,
                                                                                                        IQueryable<TRight> rightData,
                                                                                                        Expression<Func<TLeft, TProperty>> leftField,
                                                                                                        Expression<Func<TRight, TProperty>> rightField) where TLeft : class
            where TRight : class
        {
            return GetJoinQuery(leftData, rightData, leftField, rightField);
        }

        /// <summary>Джойнер по умолчанию
        ///     <para>Не учитывает этап версии</para>
        ///     <para>Устарел</para>
        /// </summary>
        /// <typeparam name="TLeft">Тип сущности в левой части</typeparam>
        /// <typeparam name="TRight">Тип сущности в правой части</typeparam>
        /// <typeparam name="TProperty">Тип свойства, по которому происходит Join</typeparam>
        /// <param name="leftData">Запрос с данными, к которым будет происходить Join</param>
        /// <param name="rightData">Запрос с данными, которые будут Join'иться</param>
        /// <param name="leftField">Выражение, которое определяет поле в левом запросе для Join'а</param>
        /// <param name="rightField">Выражение, которое определяет поле в правом запросе для Join'а</param>
        /// <returns>Коллекция после операции Join</returns>
        [Obsolete("Вместо этого метода необходимо использовать NsiToNsiJoin|RegToNsiJoin|DefaultJoin")]
        public static IQueryable<JoinResultObject<TLeft, TRight>> JoinRuleLeftOuter<TLeft, TRight, TProperty>(IQueryable<TLeft> leftData,
                                                                                                              IQueryable<TRight> rightData,
                                                                                                              Expression<Func<TLeft, TProperty>> leftField,
                                                                                                              Expression<Func<TRight, TProperty>> rightField) where TLeft : class
            where TRight : class
        {
            throw new NotImplementedException("Данный метод больше не используется. Вместо него необходимо использовать NsiToNsiJoin|RegToNsiJoin|DefaultJoin");
        }

        ///// <summary>Джойнер IEntity сущности с INsiEntity сущностью
        /////     <para>При Join'e игнорируются все результаты с VersionStage, отличным от Текущий этап</para>
        ///// </summary>
        ///// <typeparam name="TLeft">Тип сущности в левой части</typeparam>
        ///// <typeparam name="TRight">Тип сущности в правой части</typeparam>
        ///// <typeparam name="TProperty">Тип свойства, по которому происходит Join</typeparam>
        ///// <param name="leftData">Запрос с данными, к которым будет происходить Join</param>
        ///// <param name="rightData">Запрос с данными, которые будут Join'иться</param>
        ///// <param name="leftField">Выражение, которое определяет поле в левом запросе для Join'а</param>
        ///// <param name="rightField">Выражение, которое определяет поле в правом запросе для Join'а</param>
        ///// <returns>Коллекция после операции Join</returns>
        //public static IQueryable<JoinResultObject<TLeft, TRight>> JoinWithCurrentVersionStage<TLeft, TRight, TProperty>(this IQueryable<TLeft> leftData,
        //                                                                                                                IQueryable<TRight> rightData,
        //                                                                                                                Expression<Func<TLeft, TProperty>> leftField,
        //                                                                                                                Expression<Func<TRight, TProperty>> rightField)
        //    where TLeft : class, IEntity where TRight : class, INsiEntity
        //{
        //    Guard.That(leftData).IsNotNull();
        //    Guard.That(rightData).IsNotNull();
        //    Guard.That(rightField).IsNotNull();
        //    Guard.That(leftField).IsNotNull();

        //    return leftData.GroupJoin(
        //        rightData,
        //        leftField,
        //        rightField,
        //        (left, rights) => new
        //                          {
        //                              left,
        //                              rights
        //                          }).SelectMany(
        //                              groupJoin => groupJoin.rights.DefaultIfEmpty(),
        //                              (left, right) => new JoinResultObject<TLeft, TRight>
        //                                               {
        //                                                   Left = left.left,
        //                                                   Right = right
        //                                               }).Where(t => t.Right == null || t.Right.VersionStage.IsCurrent);
        //}

        ///// <summary>Джойнер сущности справочника с сущностью справочника</summary>
        ///// <typeparam name="TLeft">Тип сущности в левой части</typeparam>
        ///// <typeparam name="TRight">Тип сущности в правой части</typeparam>
        ///// <typeparam name="TProperty">Тип свойства, по которому происходит Join</typeparam>
        ///// <param name="leftData">Запрос с данными, к которым будет происходить Join</param>
        ///// <param name="rightData">Запрос с данными, которые будут Join'иться</param>
        ///// <param name="leftField">Выражение, которое определяет поле в левом запросе для Join'а</param>
        ///// <param name="rightField">Выражение, которое определяет поле в правом запросе для Join'а</param>
        ///// <returns>Коллекция после операции Join</returns>
        //public static IQueryable<JoinResultObject<TLeft, TRight>> NsiToNsiJoin<TLeft, TRight, TProperty>(this IQueryable<TLeft> leftData,
        //                                                                                                 IQueryable<TRight> rightData,
        //                                                                                                 Expression<Func<TLeft, TProperty>> leftField,
        //                                                                                                 Expression<Func<TRight, TProperty>> rightField)
        //    where TLeft : class, INsiEntity where TRight : class, INsiEntity
        //{
        //    Guard.That(leftData).IsNotNull();
        //    Guard.That(rightData).IsNotNull();
        //    Guard.That(rightField).IsNotNull();
        //    Guard.That(leftField).IsNotNull();

        //    var interpreter = new Interpreter();

        //    var rootEntities = leftData; //.Select(left => left);
        //    var directJoinQuery = rightData;
        //    var defaultJoinQuery = rightData;

        //    // Строим выражение для доступа к вложенным Dto, чтобы через него добраться до поля во вложенном GroupJoin
        //    var interpreterParameter = new Parameter("leftResult", typeof(DirectJoinResultDto<TLeft, TRight>));
        //    var propertyExpression = interpreter.Parse("leftResult.RootEntity.RootEntity", interpreterParameter).Expression;

        //    // Комбинируем получившееся выражение и выражение доступа к полю первого уровня, чтобы получить результирующую лямбду
        //    var resultExpr = ExpressionExtensions.GetCombinedExpression(propertyExpression, leftField.Body);
        //    var lambda = Expression.Lambda<Func<DirectNsiJoinResultDto<TLeft, TRight>, TProperty>>(resultExpr, interpreterParameter.Expression);

        //    #region Построение выражения для инициализации Dto, по которым в дальнейшем будет идти Equal

        //    var bindList = new List<MemberBinding>();
        //    var joinPropertyIdPropertyInfo = typeof(EqualityGroupDto<TProperty>).GetProperty("JoinPropertyId");
        //    var versionStageIdPropertyInfo = typeof(EqualityGroupDto<TProperty>).GetProperty("VersionStageId");

        //    var leftEqualityParameter = Expression.Parameter(typeof(TLeft), "leftEqualityParameter");

        //    var versionStagePropertyExpression = Expression.Property(leftEqualityParameter, "VersionStage");
        //    var versionStageIdPropertyExpression = Expression.Property(versionStagePropertyExpression, "Id");
        //    var leftFieldReplacedExpression = ExpressionExtensions.GetCombinedExpression(leftEqualityParameter, leftField.Body);

        //    bindList.Add(Expression.Bind(joinPropertyIdPropertyInfo, leftFieldReplacedExpression));
        //    bindList.Add(Expression.Bind(versionStageIdPropertyInfo, versionStageIdPropertyExpression));

        //    var leftEqualityLambda =
        //        Expression.Lambda<Func<TLeft, EqualityGroupDto<TProperty>>>(
        //            Expression.MemberInit(Expression.New(typeof(EqualityGroupDto<TProperty>)), bindList),
        //            leftEqualityParameter);

        //    bindList.Clear();
        //    var rightEqualityParameter = Expression.Parameter(typeof(TRight), "rightEqualityParameter");
        //    var rightFieldReplacedExpression = ExpressionExtensions.GetCombinedExpression(rightEqualityParameter, rightField.Body);
        //    versionStagePropertyExpression = Expression.Property(rightEqualityParameter, "VersionStage");
        //    versionStageIdPropertyExpression = Expression.Property(versionStagePropertyExpression, "Id");

        //    bindList.Add(Expression.Bind(joinPropertyIdPropertyInfo, rightFieldReplacedExpression));
        //    bindList.Add(Expression.Bind(versionStageIdPropertyInfo, versionStageIdPropertyExpression));

        //    var rightEqualityLambda =
        //        Expression.Lambda<Func<TRight, EqualityGroupDto<TProperty>>>(
        //            Expression.MemberInit(Expression.New(typeof(EqualityGroupDto<TProperty>)), bindList),
        //            rightEqualityParameter);

        //    #endregion

        //    var result = rootEntities.GroupJoin(
        //        directJoinQuery,
        //        leftEqualityLambda,
        //        rightEqualityLambda,
        //        (rootEntity, directJoinQueryGroup) => new DirectNsiJoinGroupDto<TLeft, TRight>
        //                                              {
        //                                                  RootEntity = rootEntity,
        //                                                  DirectEntities = directJoinQueryGroup
        //                                              }).SelectMany(
        //                                                  @t => @t.DirectEntities.DefaultIfEmpty(),
        //                                                  (@t, directJoin) => new DirectNsiJoinResultDto<TLeft, TRight>
        //                                                                      {
        //                                                                          RootEntity = @t,
        //                                                                          DirectEntity = directJoin
        //                                                                      }).GroupJoin(
        //                                                                          defaultJoinQuery,
        //                                                                          lambda,
        //                                                                          rightField,
        //                                                                          (@t, defaultJoinQueryGroup) => new
        //                                                                                                         {
        //                                                                                                             @t,
        //                                                                                                             defaultJoinQueryGroup
        //                                                                                                         })
        //                             .SelectMany(
        //                                 @t => @t.defaultJoinQueryGroup.DefaultIfEmpty(),
        //                                 (@t, defaultJoin) => new
        //                                                      {
        //                                                          @t,
        //                                                          defaultJoin
        //                                                      })
        //                             .Where(
        //                                 @t =>
        //                                 (@t.@t.@t.DirectEntity == null && @t.defaultJoin.VersionStage.IsCurrent) ||
        //                                 (@t.@t.@t.DirectEntity != null && @t.@t.@t.DirectEntity.Id == @t.defaultJoin.Id) ||
        //                                 ReplaceCallPlaceholder<TLeft, TProperty>(@t.@t.@t.RootEntity.RootEntity) == null)
        //                             .Select(
        //                                 @t => new JoinResultObject<TLeft, TRight>
        //                                       {
        //                                           Left = @t.@t.@t.RootEntity.RootEntity,
        //                                           Right = @t.defaultJoin
        //                                       });

        //    var resultExpression = ExpressionExtensions.ReplaceExression(
        //        result.Expression,
        //        "ReplaceCallPlaceholder",
        //        expression =>
        //        {
        //            var currentExpression = expression.Arguments[0];
        //            return Expression.Property(currentExpression, ((MemberExpression)leftField.Body).Member.Name);
        //        });
        //    return result.Provider.CreateQuery<JoinResultObject<TLeft, TRight>>(resultExpression);
        //}

        ///// <summary>Джойнер сущности реестра с сущностью справочника</summary>
        ///// <typeparam name="TLeft">Тип сущности в левой части</typeparam>
        ///// <typeparam name="TRight">Тип сущности в правой части</typeparam>
        ///// <typeparam name="TProperty">Тип свойства, по которому происходит Join</typeparam>
        ///// <param name="leftData">Запрос с данными, к которым будет происходить Join</param>
        ///// <param name="rightData">Запрос с данными, которые будут Join'иться</param>
        ///// <param name="leftField">Выражение, которое определяет поле в левом запросе для Join'а</param>
        ///// <param name="rightField">Выражение, которое определяет поле в правом запросе для Join'а</param>
        ///// <returns>Коллекция после операции Join</returns>
        //public static IQueryable<JoinResultObject<TLeft, TRight>> RegToNsiJoin<TLeft, TRight, TProperty>(this IQueryable<TLeft> leftData,
        //                                                                                                 IQueryable<TRight> rightData,
        //                                                                                                 Expression<Func<TLeft, TProperty>> leftField,
        //                                                                                                 Expression<Func<TRight, TProperty>> rightField)
        //    where TLeft : class, IEntity where TRight : class, INsiEntity
        //{
        //    Guard.That(leftData).IsNotNull();
        //    Guard.That(rightData).IsNotNull();
        //    Guard.That(rightField).IsNotNull();
        //    Guard.That(leftField).IsNotNull();

        //    var interpreter = new Interpreter();

        //    //TODO: Заменить на получение через сервис
        //    long preferredVersionStageId = 1;

        //    var rootEntities = leftData; //.Select(left => left);
        //    var directJoinQuery = rightData;
        //    var defaultJoinQuery = rightData;

        //    // Строим выражение для доступа к вложенным Dto, чтобы через него добраться до поля во вложенном GroupJoin
        //    var interpreterParameter = new Parameter("leftResult", typeof(DirectJoinResultDto<TLeft, TRight>));
        //    var propertyExpression = interpreter.Parse("leftResult.RootEntity.RootEntity", interpreterParameter).Expression;

        //    // Комбинируем получившееся выражение и выражение доступа к полю первого уровня, чтобы получить результирующую лямбду
        //    var resultExpr = ExpressionExtensions.GetCombinedExpression(propertyExpression, leftField.Body);
        //    var lambda = Expression.Lambda<Func<DirectJoinResultDto<TLeft, TRight>, TProperty>>(resultExpr, interpreterParameter.Expression);

        //    #region Построение выражения для инициализации Dto, по которым в дальнейшем будет идти Equal

        //    var bindList = new List<MemberBinding>();

        //    var joinPropertyIdPropertyInfo = typeof(EqualityGroupDto<TProperty>).GetProperty("JoinPropertyId");
        //    var leftEqualityParameter = Expression.Parameter(typeof(TLeft), "leftEqualityParameter");
        //    var leftFieldReplacedExpression = ExpressionExtensions.GetCombinedExpression(leftEqualityParameter, leftField.Body);
        //    bindList.Add(Expression.Bind(joinPropertyIdPropertyInfo, leftFieldReplacedExpression));

        //    var versionStageIdPropertyInfo = typeof(EqualityGroupDto<TProperty>).GetProperty("VersionStageId");
        //    var preferredVersionStageIdExpression = Expression.Constant(preferredVersionStageId, typeof(long));
        //    bindList.Add(Expression.Bind(versionStageIdPropertyInfo, preferredVersionStageIdExpression));

        //    var leftEqualityLambda =
        //        Expression.Lambda<Func<TLeft, EqualityGroupDto<TProperty>>>(
        //            Expression.MemberInit(Expression.New(typeof(EqualityGroupDto<TProperty>)), bindList),
        //            leftEqualityParameter);

        //    bindList.Clear();
        //    var rightEqualityParameter = Expression.Parameter(typeof(TRight), "rightEqualityParameter");
        //    var rightFieldReplacedExpression = ExpressionExtensions.GetCombinedExpression(rightEqualityParameter, rightField.Body);
        //    bindList.Add(Expression.Bind(joinPropertyIdPropertyInfo, rightFieldReplacedExpression));

        //    var versionStagePropertyExpression = Expression.Property(rightEqualityParameter, "VersionStage");
        //    var versionStageIdPropertyExpression = Expression.Property(versionStagePropertyExpression, "Id");
        //    bindList.Add(Expression.Bind(versionStageIdPropertyInfo, versionStageIdPropertyExpression));

        //    var rightEqualityLambda =
        //        Expression.Lambda<Func<TRight, EqualityGroupDto<TProperty>>>(
        //            Expression.MemberInit(Expression.New(typeof(EqualityGroupDto<TProperty>)), bindList),
        //            rightEqualityParameter);

        //    #endregion

        //    var result = rootEntities.GroupJoin(
        //        directJoinQuery,
        //        leftEqualityLambda,
        //        rightEqualityLambda,
        //        (rootEntity, directJoinQueryGroup) => new DirectJoinGroupDto<TLeft, TRight>
        //                                              {
        //                                                  RootEntity = rootEntity,
        //                                                  DirectEntities = directJoinQueryGroup
        //                                              }).SelectMany(
        //                                                  @t => @t.DirectEntities.DefaultIfEmpty(),
        //                                                  (@t, directJoin) => new DirectJoinResultDto<TLeft, TRight>
        //                                                                      {
        //                                                                          RootEntity = @t,
        //                                                                          DirectEntity = directJoin
        //                                                                      }).GroupJoin(
        //                                                                          defaultJoinQuery,
        //                                                                          lambda,
        //                                                                          rightField,
        //                                                                          (@t, defaultJoinQueryGroup) => new
        //                                                                                                         {
        //                                                                                                             @t,
        //                                                                                                             defaultJoinQueryGroup
        //                                                                                                         })
        //                             .SelectMany(
        //                                 @t => @t.defaultJoinQueryGroup.DefaultIfEmpty(),
        //                                 (@t, defaultJoin) => new
        //                                                      {
        //                                                          @t,
        //                                                          defaultJoin
        //                                                      })
        //                             .Where(
        //                                 @t =>
        //                                 (@t.@t.@t.DirectEntity == null && @t.defaultJoin.VersionStage.IsCurrent) ||
        //                                 (@t.@t.@t.DirectEntity != null && @t.@t.@t.DirectEntity.Id == @t.defaultJoin.Id) ||
        //                                 ReplaceCallPlaceholder<TLeft, TProperty>(@t.@t.@t.RootEntity.RootEntity) == null)
        //                             .Select(
        //                                 @t => new JoinResultObject<TLeft, TRight>
        //                                       {
        //                                           Left = @t.@t.@t.RootEntity.RootEntity,
        //                                           Right = @t.defaultJoin
        //                                       });

        //    var resultExpression = ExpressionExtensions.ReplaceExression(
        //        result.Expression,
        //        "ReplaceCallPlaceholder",
        //        expression =>
        //        {
        //            var currentExpression = expression.Arguments[0];
        //            return Expression.Property(currentExpression, ((MemberExpression)leftField.Body).Member.Name);
        //        });
        //    return result.Provider.CreateQuery<JoinResultObject<TLeft, TRight>>(resultExpression);
        //}

        /// <summary>Получить выражение left join</summary>
        /// <typeparam name="TLeft">Тип сущности в левой части</typeparam>
        /// <typeparam name="TRight">Тип сущности в правой части</typeparam>
        /// <typeparam name="TProperty">Тип свойства, по которому происходит Join</typeparam>
        /// <param name="leftData">Запрос с данными, к которым будет происходить Join</param>
        /// <param name="rightData">Запрос с данными, которые будут Join'иться</param>
        /// <param name="leftField">Выражение, которое определяет поле в левом запросе для Join'а</param>
        /// <param name="rightField">Выражение, которое определяет поле в правом запросе для Join'а</param>
        /// <returns>Коллекция после операции Join</returns>
        private static IQueryable<JoinResultObject<TLeft, TRight>> GetJoinQuery<TLeft, TRight, TProperty>(IQueryable<TLeft> leftData,
                                                                                                          IQueryable<TRight> rightData,
                                                                                                          Expression<Func<TLeft, TProperty>> leftField,
                                                                                                          Expression<Func<TRight, TProperty>> rightField) where TLeft : class
            where TRight : class
        {
            return leftData.GroupJoin(
                rightData,
                leftField,
                rightField,
                (left, rights) => new
                                  {
                                      left,
                                      rights
                                  }).SelectMany(
                                      groupJoin => groupJoin.rights.DefaultIfEmpty(),
                                      (left, right) => new JoinResultObject<TLeft, TRight>
                                                       {
                                                           Left = left.left,
                                                           Right = right
                                                       });
        }

        ///// <summary>Метод placeholder для последующей замены его на другое выражение</summary>
        ///// <typeparam name="TEntity">Тип сущности</typeparam>
        ///// <typeparam name="TProperty">Тип свойства</typeparam>
        ///// <param name="entity">Сущность</param>
        //private static TProperty ReplaceCallPlaceholder<TEntity, TProperty>(TEntity entity)
        //{
        //    return default(TProperty);
        //}

        //private class DirectJoinGroupDto<TLeft, TRight>
        //    where TLeft : class, IEntity
        //    where TRight : class, INsiEntity
        //{
        //    public TLeft RootEntity { get; set; }

        //    public IEnumerable<TRight> DirectEntities { get; set; }
        //}

        //private class DirectJoinResultDto<TLeft, TRight>
        //    where TLeft : class, IEntity
        //    where TRight : class, INsiEntity
        //{
        //    public DirectJoinGroupDto<TLeft, TRight> RootEntity { get; set; }

        //    public TRight DirectEntity { get; set; }
        //}

        //private class DirectNsiJoinGroupDto<TLeft, TRight>
        //    where TLeft : class, INsiEntity
        //    where TRight : class, INsiEntity
        //{
        //    public TLeft RootEntity { get; set; }

        //    public IEnumerable<TRight> DirectEntities { get; set; }
        //}

        //private class DirectNsiJoinResultDto<TLeft, TRight>
        //    where TLeft : class, INsiEntity
        //    where TRight : class, INsiEntity
        //{
        //    public DirectNsiJoinGroupDto<TLeft, TRight> RootEntity { get; set; }

        //    public TRight DirectEntity { get; set; }
        //}

        //private class EqualityGroupDto<TProperty>
        //{
        //    public TProperty JoinPropertyId { get; set; }

        //    public long VersionStageId { get; set; }
        //}
    }
}