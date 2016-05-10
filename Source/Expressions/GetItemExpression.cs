using System;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Extensions;

namespace Bars2Db.Expressions
{
    internal class GetItemExpression : Expression
    {
        private readonly Type _type;

        public GetItemExpression(Expression expression)
        {
            Expression = expression;
            _type = expression.Type.GetGenericArgumentsEx()[0];
        }

        public Expression Expression { get; }

        public override Type Type => _type;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override bool CanReduce => true;

        public override Expression Reduce()
        {
            var mi = MemberHelper.MethodOf(() => Enumerable.First<string>(null));
            var gi = mi.GetGenericMethodDefinition().MakeGenericMethod(_type);

            return Call(null, gi, Expression);
        }
    }
}