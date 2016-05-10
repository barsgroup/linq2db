using System;
using System.Linq.Expressions;

namespace Bars2Db.Expressions
{
    internal class ChangeTypeExpression : Expression
    {
        public const int ChangeTypeType = 1000;

        public ChangeTypeExpression(Expression expression, Type type)
        {
            Expression = expression;
            Type = type;
        }

        public override Type Type { get; }

        public override ExpressionType NodeType => (ExpressionType) ChangeTypeType;

        public Expression Expression { get; }

        public override string ToString()
        {
            return "(" + Type + ")" + Expression;
        }
    }
}