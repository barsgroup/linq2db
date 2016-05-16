using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Bars2Db.Extensions;
using Bars2Db.Mapping;

namespace Bars2Db.Expressions
{
    internal class ConvertFromDataReaderExpression : Expression
    {
        private static readonly MethodInfo _columnReaderGetValueInfo =
            MemberHelper.MethodOf<ColumnReader>(r => r.GetValue(null));

        private static readonly MethodInfo _isDBNullInfo = MemberHelper.MethodOf<IDataReader>(rd => rd.IsDBNull(0));
        private readonly IDataContext _dataContext;
        private readonly Expression _dataReaderParam;

        private readonly Type _type;

        public ConvertFromDataReaderExpression(
            Type type, int idx, Expression dataReaderParam, IDataContext dataContext)
        {
            _type = type;
            Idx = idx;
            _dataReaderParam = dataReaderParam;
            _dataContext = dataContext;
        }

        public override Type Type => _type;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override bool CanReduce => true;

        public int Idx { get; }

        public override Expression Reduce()
        {
            var columnReader = new ColumnReader(_dataContext, _dataContext.MappingSchema, _type, Idx);
            return Convert(Call(Constant(columnReader), _columnReaderGetValueInfo, _dataReaderParam), _type);
        }

        public Expression Reduce(IDataReader dataReader)
        {
            return GetColumnReader(_dataContext, _dataContext.MappingSchema, dataReader, _type, Idx, _dataReaderParam);
        }

        private static Expression GetColumnReader(
            IDataContext dataContext, MappingSchema mappingSchema, IDataReader dataReader, Type type, int idx,
            Expression dataReaderExpr)
        {
            var ex = dataContext.GetReaderExpression(mappingSchema, dataReader, idx, dataReaderExpr,
                type.ToNullableUnderlying());

            if (ex.NodeType == ExpressionType.Lambda)
            {
                var l = (LambdaExpression) ex;

                switch (l.Parameters.Count)
                {
                    case 1:
                        ex = l.GetBody(dataReaderExpr);
                        break;
                    case 2:
                        ex = l.GetBody(dataReaderExpr, Constant(idx));
                        break;
                }
            }

            var conv = mappingSchema.GetConvertExpression(ex.Type, type, false);

            // Replace multiple parameters with single variable or single parameter with the reader expression.
            //
            if (conv.Body.GetCount(e => e == conv.Parameters[0]) > 1)
            {
                var variable = Variable(ex.Type);
                var assign = Assign(variable, ex);

                ex = Block(new[] {variable}, assign, conv.GetBody(variable));
            }
            else
            {
                ex = conv.GetBody(ex);
            }

            // Add check null expression.
            //
            if (dataContext.IsDBNullAllowed(dataReader, idx) ?? true)
            {
                ex = Condition(
                    Call(dataReaderExpr, _isDBNullInfo, Constant(idx)),
                    Constant(mappingSchema.GetDefaultValue(type), type),
                    ex);
            }

            return ex;
        }

        private class ColumnReader
        {
            private readonly ConcurrentDictionary<Type, Func<IDataReader, object>> _columnConverters =
                new ConcurrentDictionary<Type, Func<IDataReader, object>>();

            private readonly int _columnIndex;
            private readonly Type _columnType;

            private readonly IDataContext _dataContext;
            private readonly object _defaultValue;
            private readonly MappingSchema _mappingSchema;

            public ColumnReader(IDataContext dataContext, MappingSchema mappingSchema, Type columnType, int columnIndex)
            {
                _dataContext = dataContext;
                _mappingSchema = mappingSchema;
                _columnType = columnType;
                _columnIndex = columnIndex;
                _defaultValue = mappingSchema.GetDefaultValue(columnType);
            }

            public object GetValue(IDataReader dataReader)
            {
                //var value = dataReader.GetValue(_columnIndex);

                if (dataReader.IsDBNull(_columnIndex))
                    return _defaultValue;

                var fromType = dataReader.GetFieldType(_columnIndex);

                Func<IDataReader, object> func;

                if (!_columnConverters.TryGetValue(fromType, out func))
                {
                    var parameter = Parameter(typeof(IDataReader));
                    var dataReaderExpr = Convert(parameter, dataReader.GetType());

                    var expr = GetColumnReader(_dataContext, _mappingSchema, dataReader, _columnType, _columnIndex,
                        dataReaderExpr);

                    var lex = Lambda<Func<IDataReader, object>>(
                        expr.Type == typeof(object) ? expr : Convert(expr, typeof(object)),
                        parameter);

                    _columnConverters[fromType] = func = lex.Compile();
                }

                return func(dataReader);

                /*
				var value = dataReader.GetValue(_columnIndex);

				if (value is DBNull || value == null)
					return _defaultValue;

				var fromType = value.GetType();

				if (fromType == _columnType)
					return value;

				Func<object,object> func;

				if (!_columnConverters.TryGetValue(fromType, out func))
				{
					var conv = _mappingSchema.GetConvertExpression(fromType, _columnType, false);
					var pex  = Expression.Parameter(typeof(object));
					var ex   = ReplaceParameter(conv, Expression.Convert(pex, fromType));
					var lex  = Expression.Lambda<Func<object, object>>(
						ex.Type == typeof(object) ? ex : Expression.Convert(ex, typeof(object)),
						pex);

					_columnConverters[fromType] = func = lex.Compile();
				}

				return func(value);
				*/
            }
        }
    }
}