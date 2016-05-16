using System;
using System.Collections.Generic;
using System.Text;
using Bars2Db.Extensions;
using Bars2Db.Mapping;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.SqlElements
{
    public class SqlParameter : BaseQueryElement,
        ISqlParameter
    {
        public SqlParameter(Type systemType, string name, object value)
        {
            if (systemType.ToNullableUnderlying().IsEnumEx())
                throw new ArgumentException();

            IsQueryParameter = true;
            Name = name;
            SystemType = systemType;
            RawValue = value;

            DataType = MappingSchema.Default.GetDataType(SystemType).DataType;
        }

        public SqlParameter(Type systemType, string name, object value, Func<object, object> valueConverter)
            : this(systemType, name, value)
        {
            _valueConverter = valueConverter;
        }

        public string LikeEnd { get; set; }

        public object RawValue { get; private set; }

        public string Name { get; set; }

        public Type SystemType { get; set; }

        public bool IsQueryParameter { get; set; }

        public DataType DataType { get; set; }

        public int DbSize { get; set; }

        public string LikeStart { get; set; }

        public bool ReplaceLike { get; set; }

        public object Value
        {
            get
            {
                var value = RawValue;

                if (ReplaceLike)
                {
                    value = value?.ToString().Replace("[", "[[]");
                }

                if (LikeStart != null)
                {
                    if (value != null)
                    {
                        return value.ToString().IndexOfAny(new[] {'%', '_'}) < 0
                            ? LikeStart + value + LikeEnd
                            : LikeStart + EscapeLikeText(value.ToString()) + LikeEnd;
                    }
                }

                var valueConverter = ValueConverter;
                return valueConverter == null
                    ? value
                    : valueConverter(value);
            }

            set { RawValue = value; }
        }

        #region ISqlExpression Members

        public int Precedence => SqlQuery.Precedence.Primary;

        #endregion

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            return func(this);
        }

        #endregion

        #region IEquatable<ISqlExpression> Members

        bool IEquatable<IQueryExpression>.Equals(IQueryExpression other)
        {
            if (this == other)
                return true;

            var p = other as ISqlParameter;
            return p != null && Name != null && p.Name != null && Name == p.Name && SystemType == p.SystemType;
        }

        #endregion

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
            {
                var p = new SqlParameter(SystemType, Name, RawValue, _valueConverter)
                {
                    IsQueryParameter = IsQueryParameter,
                    DataType = DataType,
                    DbSize = DbSize,
                    LikeStart = LikeStart,
                    LikeEnd = LikeEnd,
                    ReplaceLike = ReplaceLike
                };

                objectTree.Add(this, clone = p);
            }

            return clone;
        }

        #endregion

        #region Overrides

#if OVERRIDETOSTRING

        public override string ToString()
        {
            return
                ((IQueryElement) this).ToString(new StringBuilder(), new Dictionary<IQueryElement, IQueryElement>())
                    .ToString();
        }

#endif

        #endregion

        #region Value Converter

        internal List<int> TakeValues;

        private Func<object, object> _valueConverter;

        public Func<object, object> ValueConverter
        {
            get
            {
                if (_valueConverter == null && TakeValues != null)
                    foreach (var take in TakeValues.ToArray())
                        SetTakeConverter(take);

                return _valueConverter;
            }

            set { _valueConverter = value; }
        }

        public void SetTakeConverter(int take)
        {
            if (TakeValues == null)
                TakeValues = new List<int>();

            TakeValues.Add(take);

            SetTakeConverterInternal(take);
        }

        private void SetTakeConverterInternal(int take)
        {
            var conv = _valueConverter;

            if (conv == null)
                _valueConverter = v => v == null
                    ? null
                    : (object) ((int) v + take);
            else
                _valueConverter = v => v == null
                    ? null
                    : (object) ((int) conv(v) + take);
        }

        private static string EscapeLikeText(string text)
        {
            if (text.IndexOfAny(new[] {'%', '_'}) < 0)
                return text;

            var builder = new StringBuilder(text.Length);

            foreach (var ch in text)
            {
                switch (ch)
                {
                    case '%':
                    case '_':
                    case '~':
                        builder.Append('~');
                        break;
                }

                builder.Append(ch);
            }

            return builder.ToString();
        }

        #endregion

        #region ISqlExpression Members

        public bool CanBeNull()
        {
            if (SystemType == null && RawValue == null)
                return true;

            return SqlDataType.CanBeNull(SystemType ?? RawValue.GetType());
        }

        public bool Equals(IQueryExpression other, Func<IQueryExpression, IQueryExpression, bool> comparer)
        {
            return ((IQueryExpression) this).Equals(other) && comparer(this, other);
        }

        #endregion

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.SqlParameter;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            return sb.Append('@').Append(Name ?? "parameter").Append('[').Append(Value ?? "NULL").Append(']');
        }

        #endregion
    }
}