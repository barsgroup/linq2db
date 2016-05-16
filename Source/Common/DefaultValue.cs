using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Expressions;
using Bars2Db.Extensions;
using Bars2Db.Mapping;

namespace Bars2Db.Common
{
    public static class DefaultValue
    {
        private static readonly ConcurrentDictionary<Type, object> _values = new ConcurrentDictionary<Type, object>();

        static DefaultValue()
        {
            _values[typeof(int)] = default(int);
            _values[typeof(uint)] = default(uint);
            _values[typeof(byte)] = default(byte);
            _values[typeof(char)] = default(char);
            _values[typeof(bool)] = default(bool);
            _values[typeof(sbyte)] = default(sbyte);
            _values[typeof(short)] = default(short);
            _values[typeof(long)] = default(long);
            _values[typeof(ushort)] = default(ushort);
            _values[typeof(ulong)] = default(ulong);
            _values[typeof(float)] = default(float);
            _values[typeof(double)] = default(double);
            _values[typeof(decimal)] = default(decimal);
            _values[typeof(DateTime)] = default(DateTime);
            _values[typeof(TimeSpan)] = default(TimeSpan);
            _values[typeof(DateTimeOffset)] = default(DateTimeOffset);
            _values[typeof(Guid)] = default(Guid);
            _values[typeof(string)] = default(string);
        }

        public static object GetValue([Properties.NotNull] Type type, MappingSchema mappingSchema = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var ms = mappingSchema ?? MappingSchema.Default;

            object value;

            if (_values.TryGetValue(type, out value))
                return value;

            if (type.IsEnumEx())
            {
                var mapValues = ms.GetMapValues(type);

                if (mapValues != null)
                {
                    var fields =
                        from f in mapValues
                        where f.MapValues.Any(a => a.Value == null)
                        select f.OrigValue;

                    value = fields.FirstOrDefault();
                }
            }

            if (value == null && !type.IsClassEx() && !type.IsNullable())
            {
                var mi = MemberHelper.MethodOf(() => GetValue<int>());

                value =
                    Expression.Lambda<Func<object>>(
                        Expression.Convert(
                            Expression.Call(mi.GetGenericMethodDefinition().MakeGenericMethod(type)),
                            typeof(object)))
                        .Compile()();
            }

            _values[type] = value;

            return value;
        }

        public static T GetValue<T>()
        {
            object value;

            if (_values.TryGetValue(typeof(T), out value))
                return (T) value;

            _values[typeof(T)] = default(T);

            return default(T);
        }

        public static void SetValue<T>(T value)
        {
            _values[typeof(T)] = value;
        }
    }

    public static class DefaultValue<T>
    {
        private static T _value = DefaultValue.GetValue<T>();

        public static T Value
        {
            get { return _value; }
            set { DefaultValue.SetValue(_value = value); }
        }
    }
}