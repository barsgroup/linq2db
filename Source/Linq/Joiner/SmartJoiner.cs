using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bars2Db.Extensions;
using Bars2Db.Linq.Joiner.Graph;
using Bars2Db.Linq.Joiner.Interfaces;
using Bars2Db.Linq.Joiner.PropertyJoiner;
using Bars2Db.Linq.Joiner.Visitors;
using Bars2Db.Linq.Joiner.Visitors.Entities;
using QuickGraph.Algorithms;
using Seterlund.CodeGuard;
using Seterlund.CodeGuard.Validators;

namespace Bars2Db.Linq.Joiner
{
    public class SmartJoiner : ISmartJoiner
    {
        public IFullPathVisitor FullPath { get; set; }

        public IExpressionExecuteService ExpressionExecuteService { get; set; }

        public IRootQueryProvider RootQueryProvider { get; set; }

        /// <summary>Кэш, для хранения построенных делегатов</summary>
        public IJoinService JoinService { get; set; }

        public IQueryable CreateQuery(Expression expression)
        {
            var resultType = expression.Type.GetItemType() ?? expression.Type;
            var queryType = typeof(SmartQuery<>).MakeGenericType(resultType);
            return (IQueryable)Activator.CreateInstance(queryType, this, expression);
        }

        public IQueryable<TEntity> CreateQuery<TEntity>()
        {
            return new SmartQuery<TEntity>(this);
        }

        public IQueryable CreateQuery(Type resultType)
        {
            var queryType = typeof(SmartQuery<>).MakeGenericType(resultType);
            return (IQueryable)Activator.CreateInstance(queryType, this);
        }

        public IQueryable<TEntity> CreateQuery<TEntity>(Expression expression)
        {
            return new SmartQuery<TEntity>(this, expression);
        }

        public IEnumerable<TEntity> Execute<TEntity>(IQueryable<TEntity> smartQuery)
        {
            var queryWithJoines = ToQueryWithJoins(smartQuery.Expression);

            return ExpressionExecuteService.Execute<TEntity>(queryWithJoines.Expression);
        }

        public object Execute(Expression expression)
        {
            throw new NotSupportedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var queryWithJoines = ToQueryWithJoins(expression);

            var result = (TResult)ExpressionExecuteService.Execute(queryWithJoines.Expression);

            return result;
        }

        private static void AddEdgeIfNotExists(ReferenceGraph graph, ReferenceEdge referenceEdge)
        {
            if (!graph.ContainsEdge(referenceEdge))
            {
                graph.AddEdge(referenceEdge);
            }
        }

        private static void AddVertexIfNotExists(ReferenceGraph graph, ReferenceGraphVertex vertex)
        {
            if (!graph.ContainsVertex(vertex))
            {
                graph.AddVertex(vertex);
            }
        }

        private Expression ExecuteSubQuery(Expression node)
        {
            var query = (IQueryable)Expression.Lambda(node).Compile().DynamicInvoke();
            var replacer = new RestructQueryVisitor(ExecuteSubQuery, query.Expression);
            var expressionsWithJoins = replacer.Visit();

            return expressionsWithJoins;
        }

        private ReferenceGraph FillReferenceGraph(IGrouping<Expression, FullPathInfo> fullPahts)
        {
            var graph = new ReferenceGraph();

            // Создаем root вершину в графе
            graph.AddVertex(new ReferenceGraphVertex(new FullPathInfo(fullPahts.Key, new PropertyInfo[] { })));

            // Обходим получившиеся выражения для построения полного графа
            foreach (var path in fullPahts)
            {
                //TODO Исправить
                // Проверяем, что тип PropertyInfo нужно связывать
                if (!path.PropertyInfos.Any()) //|| !IsKaliningradType(path.PropertyInfos.Last()))
                {
                    continue;
                }

                var currentLevelVertex = new ReferenceGraphVertex(path);
                AddVertexIfNotExists(graph, currentLevelVertex);

                // Если это не root узел
                if (path.PropertyInfos.Any())
                {
                    // Если в коллекции типов был один единственный PropertyInfo, стало быть родительский элемент - это root
                    FullPathInfo parentPathInfo;
                    if (path.PropertyInfos.Length == 1)
                    {
                        parentPathInfo = new FullPathInfo(path.Root, new PropertyInfo[] { });
                    }
                    else
                    {
                        var parentPropInfoCollection = path.PropertyInfos.Take(path.PropertyInfos.Length - 1).ToArray();
                        parentPathInfo = new FullPathInfo(path.Root, parentPropInfoCollection);
                    }

                    var parentVertex = new ReferenceGraphVertex(parentPathInfo);
                    AddVertexIfNotExists(graph, parentVertex);

                    var referenceEdge = new ReferenceEdge(currentLevelVertex, parentVertex, path.PropertyInfos.Last());

                    AddEdgeIfNotExists(graph, referenceEdge);
                }
            }

            return graph;
        }

        /// <summary>Получение запроса для ребра графа</summary>
        /// <param name="fullPathInfo">Вершина для которой запрашиваются данные</param>
        private IQueryable GetQueryable(FullPathInfo fullPathInfo)
        {
            if (fullPathInfo.MapQueryable != null)
            {
                var queryWithJoins = ToQueryWithJoins(fullPathInfo.MapQueryable.Expression);
                return queryWithJoins;
            }

            var rootConstant = (ConstantExpression)fullPathInfo.Root;

            var vertexQueryableType = fullPathInfo.PropertyInfos.Any() // Если коллекция PropertyInfo пуста, значит это root, берем его тип
                                          ? fullPathInfo.PropertyInfos.Last().PropertyType
                                          : ((IQueryable)rootConstant.Value).ElementType;

            return RootQueryProvider.GetQuery(vertexQueryableType);
        }

        private IQueryable GetQueryWithJoins(IGrouping<Expression, FullPathInfo> fullPahts)
        {
            var referenceGraph = FillReferenceGraph(fullPahts);

            return PerformJoin(referenceGraph, fullPahts);
        }

        /// <summary>
        ///     Выбираем пути, которые по цепочке PropertyInfo начинаются также как и текущий. Так как нам нужно только
        ///     следующее поле в цепочке,
        ///     <para>то выбор идет среди путей у которых глубина на 1 больше чем у текущего пути</para>
        /// </summary>
        /// <param name="currentFullPathInfo">Текущий путь</param>
        /// <param name="allFullPathInfos">Все пути</param>
        private IEnumerable<PropertyInfo> GetSimplePropertiesForJoin(FullPathInfo currentFullPathInfo, IGrouping<Expression, FullPathInfo> allFullPathInfos)
        {
            return from fullPathInfo in allFullPathInfos.Where(f => f.GetDepth() - currentFullPathInfo.GetDepth() == 1)
                   where FullPathInfo.StartWith(fullPathInfo, currentFullPathInfo)
                   select fullPathInfo.PropertyInfos.Last();
        }

        private IQueryable JoinGraph(ReferenceGraph joinGraph, IDictionary<ReferenceGraphVertex, IQueryable> joinQueries, IGrouping<Expression, FullPathInfo> fullPahts)
        {
            Guard.That(joinGraph.VertexCount).IsEqual(joinQueries.Count);

            if (joinGraph.VertexCount == 1)
            {
                return joinQueries.First().Value;
            }

            var vertices = joinGraph.TopologicalSort();

            var joinedQueries = new Dictionary<ReferenceGraphVertex, IQueryable>();

            foreach (var vertex in vertices)
            {
                var currentOriginalData = joinQueries[vertex];

                var curVertex = vertex;
                var inEdges = joinGraph.Edges.Where(x => x.Target.Equals(curVertex));

                var dataForJoin = inEdges.ToDictionary(e => e.Tag, e => joinedQueries[e.Source]);

                var simplePropertiesForJoin = GetSimplePropertiesForJoin(vertex.FullPathInfo, fullPahts);

                var joinedQuery = JoinService.JoinData(currentOriginalData, dataForJoin, simplePropertiesForJoin.ToArray());

                joinedQueries.Add(vertex, joinedQuery);
            }

            // последние приджойненные данные это и есть результат
            return joinedQueries.Last().Value;
        }

        private IQueryable PerformJoin(ReferenceGraph referenceGraph, IGrouping<Expression, FullPathInfo> fullPahts)
        {
            IDictionary<ReferenceGraphVertex, IQueryable> queryableDictionary = new Dictionary<ReferenceGraphVertex, IQueryable>();
            foreach (var graphVertex in referenceGraph.Vertices)
            {
                queryableDictionary.Add(graphVertex, GetQueryable(graphVertex.FullPathInfo));
            }

            var joinedQuery = JoinGraph(referenceGraph, queryableDictionary, fullPahts);

            return joinedQuery;
        }

        private IQueryable ToQueryWithJoins(Expression expression)
        {
            var restructQueryVisitor = new RestructQueryVisitor(ExecuteSubQuery, expression);
            var restructedExpression = restructQueryVisitor.Visit();

            var fullPathGroups = FullPath.BuildFullPaths(restructedExpression).GroupBy(p => p.Root);

            var replaceQueryDictionary = new Dictionary<Expression, IQueryable>();

            foreach (var fullPathGroup in fullPathGroups)
            {
                var queryWithJoins = GetQueryWithJoins(fullPathGroup);
                replaceQueryDictionary.Add(fullPathGroup.Key, queryWithJoins);
            }

            var replacer = new ReplaceQueryVisitor(replaceQueryDictionary, restructedExpression);
            var expressionWithJoines = replacer.Visit();

            return CreateQuery(expressionWithJoines);
        }
    }
}