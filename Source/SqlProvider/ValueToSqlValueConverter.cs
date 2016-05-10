using System;
using System.Collections.Generic;
using System.Globalization;
using Bars2Db.Common;
using Bars2Db.Extensions;
using Bars2Db.SqlQuery.QueryElements.SqlElements;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlProvider
{
    using ConverterType = Func<ISqlDataType, object, string>;

    public class ValueToSqlValueConverter
    {
        private static readonly NumberFormatInfo NumberFormatInfo = new NumberFormatInfo
        {
            CurrencyDecimalDigits = NumberFormatInfo.InvariantInfo.CurrencyDecimalDigits,
            CurrencyDecimalSeparator = NumberFormatInfo.InvariantInfo.CurrencyDecimalSeparator,
            CurrencyGroupSeparator = NumberFormatInfo.InvariantInfo.CurrencyGroupSeparator,
            CurrencyGroupSizes = NumberFormatInfo.InvariantInfo.CurrencyGroupSizes,
            CurrencyNegativePattern = NumberFormatInfo.InvariantInfo.CurrencyNegativePattern,
            CurrencyPositivePattern = NumberFormatInfo.InvariantInfo.CurrencyPositivePattern,
            CurrencySymbol = NumberFormatInfo.InvariantInfo.CurrencySymbol,
            NaNSymbol = NumberFormatInfo.InvariantInfo.NaNSymbol,
            NegativeInfinitySymbol = NumberFormatInfo.InvariantInfo.NegativeInfinitySymbol,
            NegativeSign = NumberFormatInfo.InvariantInfo.NegativeSign,
            NumberDecimalDigits = NumberFormatInfo.InvariantInfo.NumberDecimalDigits,
            NumberDecimalSeparator = ".",
            NumberGroupSeparator = NumberFormatInfo.InvariantInfo.NumberGroupSeparator,
            NumberGroupSizes = NumberFormatInfo.InvariantInfo.NumberGroupSizes,
            NumberNegativePattern = NumberFormatInfo.InvariantInfo.NumberNegativePattern,
            PercentDecimalDigits = NumberFormatInfo.InvariantInfo.PercentDecimalDigits,
            PercentDecimalSeparator = ".",
            PercentGroupSeparator = NumberFormatInfo.InvariantInfo.PercentGroupSeparator,
            PercentGroupSizes = NumberFormatInfo.InvariantInfo.PercentGroupSizes,
            PercentNegativePattern = NumberFormatInfo.InvariantInfo.PercentNegativePattern,
            PercentPositivePattern = NumberFormatInfo.InvariantInfo.PercentPositivePattern,
            PercentSymbol = NumberFormatInfo.InvariantInfo.PercentSymbol,
            PerMilleSymbol = NumberFormatInfo.InvariantInfo.PerMilleSymbol,
            PositiveInfinitySymbol = NumberFormatInfo.InvariantInfo.PositiveInfinitySymbol,
            PositiveSign = NumberFormatInfo.InvariantInfo.PositiveSign
        };

        private readonly ValueToSqlValueConverter[] _baseConverters;
        private readonly Dictionary<Type, ConverterType> _converters = new Dictionary<Type, ConverterType>();

        private ConverterType _booleanConverter;
        private ConverterType _byteConverter;
        private ConverterType _charConverter;
        private ConverterType _dateTimeConverter;
        private ConverterType _decimalConverter;
        private ConverterType _doubleConverter;
        private ConverterType _int16Converter;
        private ConverterType _int32Converter;
        private ConverterType _int64Converter;
        private ConverterType _sByteConverter;
        private ConverterType _singleConverter;
        private ConverterType _stringConverter;
        private ConverterType _uInt16Converter;
        private ConverterType _uInt32Converter;
        private ConverterType _uInt64Converter;

        public ValueToSqlValueConverter(params ValueToSqlValueConverter[] converters)
        {
            _baseConverters = converters ?? Array<ValueToSqlValueConverter>.Empty;
        }

        internal void SetDefauls()
        {
            SetConverter(typeof(bool), (dt, v) => (bool) v ? "1" : "0");
            SetConverter(typeof(char), (dt, v) => BuildChar((char) v));
            SetConverter(typeof(sbyte), (dt, v) => ((sbyte) v).ToString());
            SetConverter(typeof(byte), (dt, v) => ((byte) v).ToString());
            SetConverter(typeof(short), (dt, v) => ((short) v).ToString());
            SetConverter(typeof(ushort), (dt, v) => ((ushort) v).ToString());
            SetConverter(typeof(int), (dt, v) => ((int) v).ToString());
            SetConverter(typeof(uint), (dt, v) => ((uint) v).ToString());
            SetConverter(typeof(long), (dt, v) => ((long) v).ToString());
            SetConverter(typeof(ulong), (dt, v) => ((ulong) v).ToString());
            SetConverter(typeof(float), (dt, v) => ((float) v).ToString(NumberFormatInfo));
            SetConverter(typeof(double), (dt, v) => ((double) v).ToString(NumberFormatInfo));
            SetConverter(typeof(decimal), (dt, v) => ((decimal) v).ToString(NumberFormatInfo));
            SetConverter(typeof(DateTime), (dt, v) => BuildDateTime((DateTime) v));
            SetConverter(typeof(string), (dt, v) => BuildString(v.ToString()));
            SetConverter(typeof(Guid), (dt, v) => string.Format(@"'{0}'", v));
        }

        private static string BuildString(string value)
        {
            return string.Format(@"'{0}'", value.Replace("'", "''"));
        }

        private static string BuildChar(char value)
        {
            return string.Format(@"'{0}'", value == '\'' ? "''" : value.ToString());
        }

        private static string BuildDateTime(DateTime value)
        {
            var format = "'{0:yyyy-MM-dd HH:mm:ss.fff}'";

            if (value.Millisecond == 0)
            {
                format = value.Hour == 0 && value.Minute == 0 && value.Second == 0
                    ? "'{0:yyyy-MM-dd}'"
                    : "'{0:yyyy-MM-dd HH:mm:ss}'";
            }

            return string.Format(format, value);
        }

        public bool TryConvert(object value, out string convertedValue)
        {
            if (value == null)
            {
                convertedValue = "NULL";
                return true;
            }

            return TryConvert(new SqlDataType(value.GetType()), value, out convertedValue);
        }

        public bool TryConvert(ISqlDataType dataType, object value, out string convertedValue)
        {
            if (value == null)
            {
                convertedValue = "NULL";
                return true;
            }

            var type = value.GetType();

            ConverterType converter = null;

            if (_converters.Count > 0 && !type.IsEnumEx())
            {
                switch (type.GetTypeCodeEx())
                {
                    case TypeCode.DBNull:
                        convertedValue = "NULL";
                        return true;
                    case TypeCode.Boolean:
                        converter = _booleanConverter;
                        break;
                    case TypeCode.Char:
                        converter = _charConverter;
                        break;
                    case TypeCode.SByte:
                        converter = _sByteConverter;
                        break;
                    case TypeCode.Byte:
                        converter = _byteConverter;
                        break;
                    case TypeCode.Int16:
                        converter = _int16Converter;
                        break;
                    case TypeCode.UInt16:
                        converter = _uInt16Converter;
                        break;
                    case TypeCode.Int32:
                        converter = _int32Converter;
                        break;
                    case TypeCode.UInt32:
                        converter = _uInt32Converter;
                        break;
                    case TypeCode.Int64:
                        converter = _int64Converter;
                        break;
                    case TypeCode.UInt64:
                        converter = _uInt64Converter;
                        break;
                    case TypeCode.Single:
                        converter = _singleConverter;
                        break;
                    case TypeCode.Double:
                        converter = _doubleConverter;
                        break;
                    case TypeCode.Decimal:
                        converter = _decimalConverter;
                        break;
                    case TypeCode.DateTime:
                        converter = _dateTimeConverter;
                        break;
                    case TypeCode.String:
                        converter = _stringConverter;
                        break;
                    default:
                        _converters.TryGetValue(type, out converter);
                        break;
                }
            }

            if (converter != null)
            {
                convertedValue = converter(dataType, value);
                return true;
            }

            if (_baseConverters.Length > 0)
                foreach (var valueConverter in _baseConverters)
                    if (valueConverter.TryConvert(dataType, value, out convertedValue))
                        return true;

            convertedValue = null;
            return false;
        }

        public string Convert(object value)
        {
            string convertedValue;
            if (!TryConvert(value, out convertedValue))
                convertedValue = value.ToString();
            return convertedValue;
        }

        public string Convert(ISqlDataType dataType, object value)
        {
            string convertedValue;
            if (!TryConvert(dataType, value, out convertedValue))
                convertedValue = value.ToString();
            return convertedValue;
        }

        public void SetConverter(Type type, ConverterType converter)
        {
            if (converter == null)
            {
                if (_converters.ContainsKey(type))
                    _converters.Remove(type);
            }
            else
            {
                _converters[type] = converter;

                switch (type.GetTypeCodeEx())
                {
                    case TypeCode.Boolean:
                        _booleanConverter = converter;
                        return;
                    case TypeCode.Char:
                        _charConverter = converter;
                        return;
                    case TypeCode.SByte:
                        _sByteConverter = converter;
                        return;
                    case TypeCode.Byte:
                        _byteConverter = converter;
                        return;
                    case TypeCode.Int16:
                        _int16Converter = converter;
                        return;
                    case TypeCode.UInt16:
                        _uInt16Converter = converter;
                        return;
                    case TypeCode.Int32:
                        _int32Converter = converter;
                        return;
                    case TypeCode.UInt32:
                        _uInt32Converter = converter;
                        return;
                    case TypeCode.Int64:
                        _int64Converter = converter;
                        return;
                    case TypeCode.UInt64:
                        _uInt64Converter = converter;
                        return;
                    case TypeCode.Single:
                        _singleConverter = converter;
                        return;
                    case TypeCode.Double:
                        _doubleConverter = converter;
                        return;
                    case TypeCode.Decimal:
                        _decimalConverter = converter;
                        return;
                    case TypeCode.DateTime:
                        _dateTimeConverter = converter;
                        return;
                    case TypeCode.String:
                        _stringConverter = converter;
                        return;
                }
            }
        }
    }
}