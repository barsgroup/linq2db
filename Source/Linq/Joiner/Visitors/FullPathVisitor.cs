using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Interfaces;
using Bars2Db.Linq.Joiner.Visitors.Entities;
using Bars2Db.Linq.Joiner.Visitors.Interfaces;

namespace Bars2Db.Linq.Joiner.Visitors
{
    /// <summary>Визитор для поиска полных путей(от начального запроса) для всех полей использующихся в запросе</summary>
    public class FullPathVisitor : IFullPathVisitor
    {
        public IEnumerable<IMethodCallHandler> MethodHandlers { get; set; }

        public IDefaultMethodHandler DefaultMethodHandler { get; set; }

        /// <summary>Строит полные пути использующиеся в выражении</summary>
        /// <param name="expressionNode">Выражение которое необходимо проанализировать</param>
        /// <returns>Полные пути всех использующихся в выражении свойств</returns>
        public IEnumerable<FullPathInfo> BuildFullPaths(Expression expressionNode)
        {
            var paths = new HashSet<FullPathInfo>();

            return CreateFullPathsInner(expressionNode, paths);
        }

        /// <summary>Применяет сопоставления к уже имеющимся путям</summary>
        /// <param name="fullPaths">Коллекция существующий путей</param>
        /// <param name="memberBingings">Коллекия сопоставлении</param>
        /// <returns>Пути получившиеся в результате выставления сопоставлений</returns>
        private HashSet<FullPathInfo> ApplyBindings(IEnumerable<FullPathInfo> fullPaths, IEnumerable<FullPathBinding> memberBingings)
        {
            var newFullPaths = new HashSet<FullPathInfo>();

            foreach (var path in fullPaths)
            {
                var bindingForPath = memberBingings.FirstOrDefault(binding => FullPathInfo.StartWith(path, binding.CurrentQueryPath));

                if (bindingForPath != null)
                {
                    var replacedPath = FullPathInfo.ReplaceStartPart(path, bindingForPath.CurrentQueryPath, bindingForPath.NewQueryPath);

                    if (path.Equals(bindingForPath.CurrentQueryPath))
                    {
                        if (path.MapQueryable != null && bindingForPath.NewQueryPath.MapQueryable != null)
                        {
                            throw new InvalidOperationException();
                            //throw new ValidationException(string.Format("Для пути '{0}' уже существует маппинг", path));
                        }
                        replacedPath.MapQueryable = bindingForPath.NewQueryPath.MapQueryable;
                    }

                    replacedPath.MapQueryable = path.MapQueryable ?? replacedPath.MapQueryable;

                    newFullPaths.Add(replacedPath);
                }
            }
            return newFullPaths;
        }

        /// <summary>Формирует существующие пути на каждом уровне выражения</summary>
        /// <param name="expressionNode">Текущий узел выражения</param>
        /// <param name="paths">Пути имеющиеся на данный момент</param>
        /// <returns>Пути после обработки узла</returns>
        private IEnumerable<FullPathInfo> CreateFullPathsInner(Expression expressionNode, ISet<FullPathInfo> paths)
        {
            //останавливаемся на данный момент когда нашли константу
            if (expressionNode.NodeType == ExpressionType.Constant)
            {
                paths.Add(FullPathInfo.CreatePath(expressionNode));
                return paths;
            }

            var methodCall = (MethodCallExpression)expressionNode;

            var handler = GetNodeHandler(methodCall);

            var bindings = handler.GetPathBindings(methodCall);

            paths = ApplyBindings(paths, bindings);

            var members = handler.GetPaths(methodCall);

            paths.UnionWith(members);

            var callExpressions = handler.GetNextQueries(methodCall).ToArray();

            if (!callExpressions.Any())
            {
                return paths;
            }

            var hashSetResult = new HashSet<FullPathInfo>();

            foreach (var root in callExpressions)
            {
                var pathsFromRoot = new HashSet<FullPathInfo>(paths.Where(x => x.Root == root));

                var oneRootResult = CreateFullPathsInner(root, pathsFromRoot);

                hashSetResult.UnionWith(oneRootResult);
            }

            return hashSetResult;
        }

        /// <summary>Возвращает обработчик для текущего узла</summary>
        /// <param name="expression">Текущий узел</param>
        /// <returns>Обработчик для узла</returns>
        private IMethodCallHandler GetNodeHandler(MethodCallExpression expression)
        {
            var handler = MethodHandlers.SingleOrDefault(x => x.CanHandle(expression));

            return handler ?? DefaultMethodHandler;
        }
    }
}