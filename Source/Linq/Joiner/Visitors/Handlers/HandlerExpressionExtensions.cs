using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Bars2Db.Linq.Joiner.Visitors.Handlers
{
    public static class HandlerExpressionExtensions
    {
        public static IEnumerable<MemberInfo> GetMembersFromChain(this Expression expression)
        {
            var chainDescriptor = expression.GetMemberChainDescriptor();

            if (chainDescriptor == null)
            {
                return null;
            }

            return chainDescriptor.Members;
        }

        public static ParameterExpression GetRootParameter(this Expression node)
        {
            var root = node.GetMemberChainDescriptor();

            if (root == null)
            {
                return null;
            }

            return root.RootParameter;
        }

        public static Expression ToMemberPath(this MemberInfo property, Expression query)
        {
            var elementType = query.Type.GetGenericArguments().First();

            var memberPath = Expression.MakeMemberAccess(Expression.Parameter(elementType, elementType.Name), property);

            return memberPath;
        }

        private static MemberChainDescriptor GetMemberChainDescriptor(this Expression node)
        {
            if (node.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression)node;
                
                return memberExpression.GetMemberChainDescriptor();
            }

            if (node.NodeType == ExpressionType.Parameter)
            {
                return new MemberChainDescriptor
                       {
                           RootParameter = (ParameterExpression)node,
                           Members = new MemberInfo[] { }
                       };
            }

            return null;
        }

        private static MemberChainDescriptor GetMemberChainDescriptor(this MemberExpression node)
        {
            var properties = new List<MemberInfo>();

            var iterator = node;

            while (iterator.Expression != null)
            {
                properties.Add(iterator.Member);

                var expression = iterator.Expression;

                if (expression.NodeType == ExpressionType.Convert)
                {
                    expression = ((UnaryExpression)expression).Operand;
                }

                if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    iterator = (MemberExpression)expression;
                }
                else
                {
                    //нашли рут
                    var rootOfMemberExpressionChain = expression as ParameterExpression;

                    if (rootOfMemberExpressionChain == null)
                    {
                        return null;
                    }
                    properties.Reverse();
                    return new MemberChainDescriptor
                           {
                               Members = properties.ToArray(),
                               RootParameter = rootOfMemberExpressionChain
                           };
                }
            }

            //ничего не нашли. такое случается при статических полях
            return null;
        }

        private class MemberChainDescriptor
        {
            public ParameterExpression RootParameter { get; set; }

            public MemberInfo[] Members { get; set; }
        }
    }
}