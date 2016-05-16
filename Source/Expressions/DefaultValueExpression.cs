using System;
using System.Linq.Expressions;
using Bars2Db.Common;
using Bars2Db.Mapping;

namespace Bars2Db.Expressions
{
    public class DefaultValueExpression : Expression
    {
        private readonly MappingSchema _mappingSchema;

        public DefaultValueExpression(MappingSchema mappingSchema, Type type)
        {
            _mappingSchema = mappingSchema;
            Type = type;
        }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override bool CanReduce => true;

        public override Expression Reduce()
        {
            return Constant(
                _mappingSchema == null
                    ? DefaultValue.GetValue(Type)
                    : _mappingSchema.GetDefaultValue(Type),
                Type);
        }
    }
}