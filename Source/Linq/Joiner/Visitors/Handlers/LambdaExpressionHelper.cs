using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Visitors.Entities;
using Bars2Db.Linq.Joiner.Visitors.Interfaces;
using Seterlund.CodeGuard;
using Seterlund.CodeGuard.Validators;

namespace Bars2Db.Linq.Joiner.Visitors.Handlers
{
    /// <summary>Вспопогательный сервис для обработки LambdaExpression для MethodHandler</summary>
    public class LambdaExpressionHelper : ILambdaExpressionHelper
    {
        /// <summary>Возвращает всу пути обращений к полям из lambda</summary>
        public IEnumerable<FullPathInfo> GetAllMemberAccessPaths(LambdaExpression selector, Expression[] roots)
        {
            var visitor = new MembersVisitor();

            var paramsToRoots = ToRootBindingDictionary(selector, roots);

            visitor.Visit(selector);

            IEnumerable<MemberExpression> memberExpressions = visitor.MemberExpressions;

            memberExpressions = FilterByParameters(selector, memberExpressions);

            foreach (var membersFromLambda in memberExpressions)
            {
                Expression queryRoot;

                if (paramsToRoots.TryGetValue(membersFromLambda.GetRootParameter(), out queryRoot))
                {
                    yield return CreatePath(queryRoot, membersFromLambda);
                }
            }
        }

        private IDictionary<ParameterExpression, Expression> ToRootBindingDictionary(LambdaExpression lambdaExpression, Expression[] roots)
        {
            var parameters = lambdaExpression.Parameters;

            var parameterToRootBinding = new Dictionary<ParameterExpression, Expression>();

            for (var i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null)
                {
                    parameterToRootBinding[parameters[i]] = roots[i];
                }
            }

            return parameterToRootBinding;
        }

        public virtual IEnumerable<FullPathBinding> GetBindingsFromResultSelector(MethodCallExpression currentRoot, LambdaExpression selector, Expression[] roots)
        {
            var paramsToRoots = ToRootBindingDictionary(selector, roots);

            if (!IsNewSelect(selector))
            {
                var body = GetSelectBody(selector);
                var rootParameter = body.GetRootParameter();

                //если rootParameter не удалось получить то выборка не является выборкой какого-либо поля
                if (rootParameter != null)
                {
                    var rootOfParameter = paramsToRoots[rootParameter];

                    return new[] { CreateFullPathBinding(currentRoot, rootOfParameter, null, body) };
                }

                return Enumerable.Empty<FullPathBinding>();
            }

            var resultBindings = new List<FullPathBinding>();

            var bindings = GetSelectPropertyBindings(selector).ToArray();

            foreach (var parameter in selector.Parameters)
            {
                Expression root;

                if (paramsToRoots.TryGetValue(parameter, out root))
                {
                    var parameterBindings = bindings.Where(x => x.ValueExpression.GetRootParameter() == parameter);

                    var fullPathBingins = ToFullPathBindings(currentRoot, root, parameterBindings);

                    resultBindings.AddRange(fullPathBingins);
                }
            }

            return resultBindings;
        }

        public FullPathBinding CreateDefaultBinding(MethodCallExpression currentRoot)
        {
            return CreateFullPathBinding(currentRoot, currentRoot.Arguments[0], null, null);
        }

        private static bool CheckRootParameter(LambdaExpression select, Expression expression)
        {
            return select.Parameters.Contains(expression.GetRootParameter());
        }

        /// <summary>Задает связь между текущим путем и новым путем</summary>
        private FullPathBinding CreateFullPathBinding(Expression currentRoot, Expression newRoot, Expression currentMemberPath, Expression newMemberPath)
        {
            return CreateFullPathBinding(CreatePath(currentRoot, currentMemberPath), CreatePath(newRoot, newMemberPath));
        }

        /// <summary>Задает связь между текущим путем и новым путем</summary>
        private FullPathBinding CreateFullPathBinding(FullPathInfo oldPath, FullPathInfo newPath)
        {
            return FullPathBinding.Create(oldPath, newPath);
        }

        private FullPathInfo CreatePath(Expression currentRoot, Expression currentMemberPath = null)
        {
            return FullPathInfo.CreatePath(currentRoot, currentMemberPath);
        }

        private static IEnumerable<MemberExpression> FilterByParameters(LambdaExpression lambda, IEnumerable<MemberExpression> memberExpressions)
        {
            var result = memberExpressions.Where(memberExpression => CheckRootParameter(lambda, memberExpression));

            return result;
        }

        private static IEnumerable<PropertyBindingInfo> FilterByParameters(LambdaExpression lambda, IEnumerable<PropertyBindingInfo> propertyBindings)
        {
            var result = propertyBindings.Where(propertyBindingInfo => CheckRootParameter(lambda, propertyBindingInfo.ValueExpression));

            return result;
        }

        /// <summary>Возвращает соотвествия </summary>
        private IEnumerable<PropertyBindingInfo> GetMemberInitBindings(MemberInitExpression bindingNode)
        {
            Guard.That(bindingNode.Bindings.All(x => x.BindingType == MemberBindingType.Assignment)).IsTrue();

            var bindings = new List<PropertyBindingInfo>();

            foreach (var memberBinding in bindingNode.Bindings.Cast<MemberAssignment>())
            {
                bindings.Add(new PropertyBindingInfo(memberBinding.Member, memberBinding.Expression));
            }

            return bindings;
        }

        /// <summary>Возвращает соотвествия между новым выражением и предыдущим </summary>
        /// <param name="newExpressionNode"> NewExpression который создает новый объект</param>
        /// <returns>Коллекция соответствий между старыми и новыми путями</returns>
        private IEnumerable<PropertyBindingInfo> GetNewBindings(NewExpression newExpressionNode)
        {
            var args = newExpressionNode.Arguments;

            var newBindings = new List<PropertyBindingInfo>();

            for (var i = 0; i < args.Count; i++)
            {
                var value = args[i];

                var newMember = newExpressionNode.Members[i];

                newBindings.Add(new PropertyBindingInfo(newMember, value));
            }

            return newBindings;
        }

        /// <summary>Возвращает тело lambda запроса</summary>
        private Expression GetSelectBody(LambdaExpression selectLambda)
        {
            return selectLambda.Body;
        }

        /// <summary>Возвращает биндинги свойств из полей с текущего selectLambda</summary>
        private IEnumerable<PropertyBindingInfo> GetSelectPropertyBindings(LambdaExpression selectLambda)
        {
            Guard.That(selectLambda.Body.NodeType).IsOneOf(new[] { ExpressionType.New, ExpressionType.MemberInit });

            IEnumerable<PropertyBindingInfo> bindings;

            if (selectLambda.Body.NodeType == ExpressionType.New)
            {
                bindings = GetNewBindings((NewExpression)selectLambda.Body);
            }
            else
            {
                bindings = GetMemberInitBindings((MemberInitExpression)selectLambda.Body);
            }

            return FilterByParameters(selectLambda, bindings);
        }

        /// <summary>Проверка содержит ли lambda оператор New</summary>
        private bool IsNewSelect(LambdaExpression selectLambda)
        {
            return selectLambda.Body.NodeType == ExpressionType.New || selectLambda.Body.NodeType == ExpressionType.MemberInit;
        }

        /// <summary>Создает биндинги путей из биндингов свойств</summary>
        /// <param name="currentRoot">Текущий рут</param>
        /// <param name="nextRoot">Следующий рут</param>
        /// <param name="propertyBindings">Биндинги свойств</param>
        private IEnumerable<FullPathBinding> ToFullPathBindings(MethodCallExpression currentRoot, Expression nextRoot, IEnumerable<PropertyBindingInfo> propertyBindings)
        {
            foreach (var propertyBinding in propertyBindings.Where(x => x.ValueExpression.GetRootParameter() != null))
            {
                var newMemberExpression = propertyBinding.Property.ToMemberPath(currentRoot);

                var expression = propertyBinding.ValueExpression;

                yield return CreateFullPathBinding(currentRoot, nextRoot, newMemberExpression, expression);
            }
        }

        /// <summary>Находит все используемые в lambda цепочки обращений к полям</summary>
        private class MembersVisitor : ExpressionVisitor
        {
            public List<MemberExpression> MemberExpressions { get; private set; }

            public MembersVisitor()
            {
                MemberExpressions = new List<MemberExpression>();
            }

            /// <summary>Visits the children of the <see cref="T:System.Linq.Expressions.MemberExpression" />.</summary>
            /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
            /// <param name="node">The expression to visit.</param>
            protected override Expression VisitMember(MemberExpression node)
            {
                var expression = node.GetRootParameter();

                if (expression != null)
                {
                    MemberExpressions.Add(node);
                }

                return base.VisitMember(node);
            }
        }
    }
}