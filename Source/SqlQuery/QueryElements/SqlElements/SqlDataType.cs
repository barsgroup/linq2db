namespace LinqToDB.SqlQuery.QueryElements.SqlElements
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Linq;
    using System.Text;

    using LinqToDB.Extensions;
    using LinqToDB.Properties;
    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class SqlDataType : BaseQueryElement,
                               ISqlDataType
    {
        #region Init

        public SqlDataType(DataType dbType)
        {
            var defaultType = GetDataType(dbType);

            DataType  = dbType;
            Type      = defaultType.Type;
            Length    = defaultType.Length;
            Precision = defaultType.Precision;
            Scale     = defaultType.Scale;
        }

        public SqlDataType(DataType dbType, int? length)
        {
            DataType = dbType;
            Type     = GetDataType(dbType).Type;
            Length   = length;
        }

        public SqlDataType(DataType dbType, int? precision, int? scale)
        {
            DataType  = dbType;
            Type      = GetDataType(dbType).Type;
            Precision = precision;
            Scale     = scale;
        }

        public SqlDataType([NotNull]Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var defaultType = GetDataType(type);

            DataType  = defaultType.DataType;
            Type      = type;
            Length    = defaultType.Length;
            Precision = defaultType.Precision;
            Scale     = defaultType.Scale;
        }

        public SqlDataType([NotNull] Type type, int length)
        {
            if (type   == null) throw new ArgumentNullException      (nameof(type));
            if (length <= 0)    throw new ArgumentOutOfRangeException(nameof(length));

            DataType = GetDataType(type).DataType;
            Type     = type;
            Length   = length;
        }

        public SqlDataType([NotNull] Type type, int precision, int scale)
        {
            if (type  == null)  throw new ArgumentNullException      (nameof(type));
            if (precision <= 0) throw new ArgumentOutOfRangeException(nameof(precision));
            if (scale     <  0) throw new ArgumentOutOfRangeException(nameof(scale));

            DataType  = GetDataType(type).DataType;
            Type      = type;
            Precision = precision;
            Scale     = scale;
        }

        public SqlDataType(DataType dbType, [NotNull]Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var defaultType = GetDataType(dbType);

            DataType  = dbType;
            Type      = type;
            Length    = defaultType.Length;
            Precision = defaultType.Precision;
            Scale     = defaultType.Scale;
        }

        public SqlDataType(DataType dbType, [NotNull] Type type, int length)
        {
            if (type   == null) throw new ArgumentNullException      (nameof(type));
            if (length <= 0)    throw new ArgumentOutOfRangeException(nameof(length));

            DataType = dbType;
            Type     = type;
            Length   = length;
        }

        public SqlDataType(DataType dbType, [NotNull] Type type, int precision, int scale)
        {
            if (type  == null)  throw new ArgumentNullException      (nameof(type));
            if (precision <= 0) throw new ArgumentOutOfRangeException(nameof(precision));
            if (scale     <  0) throw new ArgumentOutOfRangeException(nameof(scale));

            DataType  = dbType;
            Type      = type;
            Precision = precision;
            Scale     = scale;
        }

        #endregion

        #region Public Members

        public DataType DataType  { get; private set; }
        public Type     Type      { get; private set; }
        public int?     Length    { get; private set; }
        public int?     Precision { get; private set; }
        public int?     Scale     { get; private set; }

        public static readonly ISqlDataType Undefined = new SqlDataType(DataType.Undefined, typeof(object), null, null, null);

        public bool IsCharDataType
        {
            get
            {
                switch (DataType)
                {
                    case DataType.Char     :
                    case DataType.NChar    :
                    case DataType.VarChar  :
                    case DataType.NVarChar : return true;
                    default                : return false;
                }
            }
        }

        #endregion

        #region Static Members

        struct TypeInfo
        {
            public TypeInfo(DataType dbType, int maxLength, int maxPrecision, int maxScale, int maxDisplaySize)
            {
                DataType       = dbType;
                MaxLength      = maxLength;
                MaxPrecision   = maxPrecision;
                MaxScale       = maxScale;
                MaxDisplaySize = maxDisplaySize;
            }

            public readonly DataType DataType;
            public readonly int      MaxLength;
            public readonly int      MaxPrecision;
            public readonly int      MaxScale;
            public readonly int      MaxDisplaySize;
        }

        static TypeInfo[] SortTypeInfo(params TypeInfo[] info)
        {
            var sortedInfo = new TypeInfo[info.Max(ti => (int)ti.DataType) + 1];

            foreach (var typeInfo in info)
                sortedInfo[(int)typeInfo.DataType] = typeInfo;

            return sortedInfo;
        }

        static int Len(object obj)
        {
            return obj.ToString().Length;
        }

        static readonly TypeInfo[] _typeInfo = SortTypeInfo
        (
            //           DbType                 MaxLength           MaxPrecision               MaxScale       MaxDisplaySize
            //
            new TypeInfo(DataType.Int64,                8,   Len( long.MaxValue),                     0,     Len( long.MinValue)),
            new TypeInfo(DataType.Int32,                4,   Len(  int.MaxValue),                     0,     Len(  int.MinValue)),
            new TypeInfo(DataType.Int16,                2,   Len(short.MaxValue),                     0,     Len(short.MinValue)),
            new TypeInfo(DataType.Byte,                 1,   Len( byte.MaxValue),                     0,     Len( byte.MaxValue)),
            new TypeInfo(DataType.Boolean,              1,                     1,                     0,                       1),

            new TypeInfo(DataType.Decimal,             17, Len(decimal.MaxValue), Len(decimal.MaxValue), Len(decimal.MinValue)+1),
            new TypeInfo(DataType.Money,                8,                    19,                     4,                  19 + 2),
            new TypeInfo(DataType.SmallMoney,           4,                    10,                     4,                  10 + 2),
            new TypeInfo(DataType.Double,               8,                    15,                    15,              15 + 2 + 5),
            new TypeInfo(DataType.Single,               4,                     7,                     7,               7 + 2 + 4),

            new TypeInfo(DataType.DateTime,             8,                    -1,                    -1,                      23),
            new TypeInfo(DataType.DateTime2,            8,                    -1,                    -1,                      27),
            new TypeInfo(DataType.SmallDateTime,        4,                    -1,                    -1,                      19),
            new TypeInfo(DataType.Date,                 3,                    -1,                    -1,                      10),
            new TypeInfo(DataType.Time,                 5,                    -1,                    -1,                      16),
            new TypeInfo(DataType.DateTimeOffset,      10,                    -1,                    -1,                      34),

            new TypeInfo(DataType.Char,              8000,                    -1,                    -1,                    8000),
            new TypeInfo(DataType.VarChar,           8000,                    -1,                    -1,                    8000),
            new TypeInfo(DataType.Text,      int.MaxValue,                    -1,                    -1,            int.MaxValue),
            new TypeInfo(DataType.NChar,             4000,                    -1,                    -1,                    4000),
            new TypeInfo(DataType.NVarChar,          4000,                    -1,                    -1,                    4000),
            new TypeInfo(DataType.NText,     int.MaxValue,                    -1,                    -1,        int.MaxValue / 2),

            new TypeInfo(DataType.Binary,            8000,                    -1,                    -1,                      -1),
            new TypeInfo(DataType.VarBinary,         8000,                    -1,                    -1,                      -1),
            new TypeInfo(DataType.Image,     int.MaxValue,                    -1,                    -1,                      -1),

            new TypeInfo(DataType.Timestamp,            8,                    -1,                    -1,                      -1),
            new TypeInfo(DataType.Guid,                16,                    -1,                    -1,                      36),

            new TypeInfo(DataType.Variant,             -1,                    -1,                    -1,                      -1),
            new TypeInfo(DataType.Xml,                 -1,                    -1,                    -1,                      -1),
            new TypeInfo(DataType.Udt,                 -1,                    -1,                    -1,                      -1)
        );

        public static int GetMaxLength     (DataType dbType) { return _typeInfo[(int)dbType].MaxLength;      }
        public static int GetMaxPrecision  (DataType dbType) { return _typeInfo[(int)dbType].MaxPrecision;   }
        public static int GetMaxScale      (DataType dbType) { return _typeInfo[(int)dbType].MaxScale;       }
        public static int GetMaxDisplaySize(DataType dbType) { return _typeInfo[(int)dbType].MaxDisplaySize; }

        public static ISqlDataType GetDataType(Type type)
        {
            var underlyingType = type;

            if (underlyingType.IsGenericTypeEx() && underlyingType.GetGenericTypeDefinition() == typeof(Nullable<>))
                underlyingType = underlyingType.GetGenericArgumentsEx()[0];

            if (underlyingType.IsEnumEx())
                underlyingType = Enum.GetUnderlyingType(underlyingType);

            switch (underlyingType.GetTypeCodeEx())
            {
                case TypeCode.Boolean  : return Boolean;
                case TypeCode.Char     : return Char;
                case TypeCode.SByte    : return SByte;
                case TypeCode.Byte     : return Byte;
                case TypeCode.Int16    : return Int16;
                case TypeCode.UInt16   : return UInt16;
                case TypeCode.Int32    : return Int32;
                case TypeCode.UInt32   : return UInt32;
                case TypeCode.Int64    : return DbInt64;
                case TypeCode.UInt64   : return UInt64;
                case TypeCode.Single   : return Single;
                case TypeCode.Double   : return Double;
                case TypeCode.Decimal  : return Decimal;
                case TypeCode.DateTime : return DateTime;
                case TypeCode.String   : return String;
                case TypeCode.Object   :
                    if (underlyingType == typeof(Guid))           return Guid;
                    if (underlyingType == typeof(byte[]))         return ByteArray;
                    if (underlyingType == typeof(System.Data.Linq.Binary)) return LinqBinary;
                    if (underlyingType == typeof(char[]))         return CharArray;
                    if (underlyingType == typeof(DateTimeOffset)) return DateTimeOffset;
                    if (underlyingType == typeof(TimeSpan))       return TimeSpan;
                    break;
            }

#if !SILVERLIGHT && !NETFX_CORE

            if (underlyingType == typeof(SqlByte))     return SqlByte;
            if (underlyingType == typeof(SqlInt16))    return SqlInt16;
            if (underlyingType == typeof(SqlInt32))    return SqlInt32;
            if (underlyingType == typeof(SqlInt64))    return SqlInt64;
            if (underlyingType == typeof(SqlSingle))   return SqlSingle;
            if (underlyingType == typeof(SqlBoolean))  return SqlBoolean;
            if (underlyingType == typeof(SqlDouble))   return SqlDouble;
            if (underlyingType == typeof(SqlDateTime)) return SqlDateTime;
            if (underlyingType == typeof(SqlDecimal))  return SqlDecimal;
            if (underlyingType == typeof(SqlMoney))    return SqlMoney;
            if (underlyingType == typeof(SqlString))   return SqlString;
            if (underlyingType == typeof(SqlBinary))   return SqlBinary;
            if (underlyingType == typeof(SqlGuid))     return SqlGuid;
            if (underlyingType == typeof(SqlBytes))    return SqlBytes;
            if (underlyingType == typeof(SqlChars))    return SqlChars;
            if (underlyingType == typeof(SqlXml))      return SqlXml;

#endif

            return DbVariant;
        }

        public static ISqlDataType GetDataType(DataType type)
        {
            switch (type)
            {
                case DataType.Int64            : return DbInt64;
                case DataType.Binary           : return DbBinary;
                case DataType.Boolean          : return DbBoolean;
                case DataType.Char             : return DbChar;
                case DataType.DateTime         : return DbDateTime;
                case DataType.Decimal          : return DbDecimal;
                case DataType.Double           : return DbDouble;
                case DataType.Image            : return DbImage;
                case DataType.Int32            : return DbInt32;
                case DataType.Money            : return DbMoney;
                case DataType.NChar            : return DbNChar;
                case DataType.NText            : return DbNText;
                case DataType.NVarChar         : return DbNVarChar;
                case DataType.Single           : return DbSingle;
                case DataType.Guid             : return DbGuid;
                case DataType.SmallDateTime    : return DbSmallDateTime;
                case DataType.Int16            : return DbInt16;
                case DataType.SmallMoney       : return DbSmallMoney;
                case DataType.Text             : return DbText;
                case DataType.Timestamp        : return DbTimestamp;
                case DataType.Byte             : return DbByte;
                case DataType.VarBinary        : return DbVarBinary;
                case DataType.VarChar          : return DbVarChar;
                case DataType.Variant          : return DbVariant;
#if !SILVERLIGHT && !NETFX_CORE
                case DataType.Xml              : return DbXml;
#endif
                case DataType.Udt              : return DbUdt;
                case DataType.Date             : return DbDate;
                case DataType.Time             : return DbTime;
                case DataType.DateTime2        : return DbDateTime2;
                case DataType.DateTimeOffset   : return DbDateTimeOffset;
            }

            throw new InvalidOperationException();
        }

        public static bool CanBeNull(Type type)
        {
            if (type.IsValueTypeEx() == false ||
                type.IsGenericTypeEx() && type.GetGenericTypeDefinition() == typeof(Nullable<>)
#if !SILVERLIGHT && !NETFX_CORE
                || typeof(INullable).IsSameOrParentOf(type)
#endif
                )
                return true;

            return false;
        }

        #endregion

        #region Default Types

        internal SqlDataType(DataType dbType, Type type, int? length, int? precision, int? scale)
        {
            DataType  = dbType;
            Type      = type;
            Length    = length;
            Precision = precision;
            Scale     = scale;
        }

        SqlDataType(DataType dbType, Type type, Func<DataType,int> length, int precision, int scale)
            : this(dbType, type, length(dbType), precision, scale)
        {
        }

        SqlDataType(DataType dbType, Type type, int length, Func<DataType,int> precision, int scale)
            : this(dbType, type, length, precision(dbType), scale)
        {
        }

        public static readonly ISqlDataType DbInt64          = new SqlDataType(DataType.Int64,          typeof(long),                  0, 0,                0);
        public static readonly ISqlDataType DbInt32          = new SqlDataType(DataType.Int32,          typeof(int),                  0, 0,                0);
        public static readonly ISqlDataType DbInt16          = new SqlDataType(DataType.Int16,          typeof(short),                  0, 0,                0);
        public static readonly ISqlDataType DbSByte          = new SqlDataType(DataType.SByte,          typeof(sbyte),                  0, 0,                0);
        public static readonly ISqlDataType DbByte           = new SqlDataType(DataType.Byte,           typeof(byte),                   0, 0,                0);
        public static readonly ISqlDataType DbBoolean        = new SqlDataType(DataType.Boolean,        typeof(bool),                0, 0,                0);
                               
        public static readonly ISqlDataType DbDecimal        = new SqlDataType(DataType.Decimal,        typeof(decimal),                0, GetMaxPrecision, 10);
        public static readonly ISqlDataType DbMoney          = new SqlDataType(DataType.Money,          typeof(decimal),                0, GetMaxPrecision,  4);
        public static readonly ISqlDataType DbSmallMoney     = new SqlDataType(DataType.SmallMoney,     typeof(decimal),                0, GetMaxPrecision,  4);
        public static readonly ISqlDataType DbDouble         = new SqlDataType(DataType.Double,         typeof(double),                 0,               0,  0);
        public static readonly ISqlDataType DbSingle         = new SqlDataType(DataType.Single,         typeof(float),                 0,               0,  0);
                               
        public static readonly ISqlDataType DbDateTime       = new SqlDataType(DataType.DateTime,       typeof(DateTime),               0,               0,  0);
        public static readonly ISqlDataType DbDateTime2      = new SqlDataType(DataType.DateTime2,      typeof(DateTime),               0,               0,  0);
        public static readonly ISqlDataType DbSmallDateTime  = new SqlDataType(DataType.SmallDateTime,  typeof(DateTime),               0,               0,  0);
        public static readonly ISqlDataType DbDate           = new SqlDataType(DataType.Date,           typeof(DateTime),               0,               0,  0);
        public static readonly ISqlDataType DbTime           = new SqlDataType(DataType.Time,           typeof(TimeSpan),               0,               0,  0);
        public static readonly ISqlDataType DbDateTimeOffset = new SqlDataType(DataType.DateTimeOffset, typeof(DateTimeOffset),         0,               0,  0);
                               
        public static readonly ISqlDataType DbChar           = new SqlDataType(DataType.Char,           typeof(string),      GetMaxLength,               0,  0);
        public static readonly ISqlDataType DbVarChar        = new SqlDataType(DataType.VarChar,        typeof(string),      GetMaxLength,               0,  0);
        public static readonly ISqlDataType DbText           = new SqlDataType(DataType.Text,           typeof(string),      GetMaxLength,               0,  0);
        public static readonly ISqlDataType DbNChar          = new SqlDataType(DataType.NChar,          typeof(string),      GetMaxLength,               0,  0);
        public static readonly ISqlDataType DbNVarChar       = new SqlDataType(DataType.NVarChar,       typeof(string),      GetMaxLength,               0,  0);
        public static readonly ISqlDataType DbNText          = new SqlDataType(DataType.NText,          typeof(string),      GetMaxLength,               0,  0);
                               
        public static readonly ISqlDataType DbBinary         = new SqlDataType(DataType.Binary,         typeof(byte[]),      GetMaxLength,               0,  0);
        public static readonly ISqlDataType DbVarBinary      = new SqlDataType(DataType.VarBinary,      typeof(byte[]),      GetMaxLength,               0,  0);
        public static readonly ISqlDataType DbImage          = new SqlDataType(DataType.Image,          typeof(byte[]),      GetMaxLength,               0,  0);
                               
        public static readonly ISqlDataType DbTimestamp      = new SqlDataType(DataType.Timestamp,      typeof(byte[]),                 0,               0,  0);
        public static readonly ISqlDataType DbGuid           = new SqlDataType(DataType.Guid,           typeof(Guid),                   0,               0,  0);
                               
        public static readonly ISqlDataType DbVariant        = new SqlDataType(DataType.Variant,        typeof(object),                 0,               0,  0);
#if !SILVERLIGHT && !NETFX_COREI
        public static readonly ISqlDataType DbXml            = new SqlDataType(DataType.Xml,            typeof(SqlXml),                 0,               0,  0);
#endif                         
        public static readonly ISqlDataType DbUdt            = new SqlDataType(DataType.Udt,            typeof(object),                 0,               0,  0);
                               
        public static readonly ISqlDataType Boolean          = DbBoolean;
        public static readonly ISqlDataType Char             = new SqlDataType(DataType.Char,           typeof(char),                   1,               0,  0);
        public static readonly ISqlDataType SByte            = DbSByte;
        public static readonly ISqlDataType Byte             = DbByte;
        public static readonly ISqlDataType Int16            = DbInt16;
        public static readonly ISqlDataType UInt16           = new SqlDataType(DataType.UInt16,         typeof(ushort),                 0,               0,  0);
        public static readonly ISqlDataType Int32            = DbInt32;
        public static readonly ISqlDataType UInt32           = new SqlDataType(DataType.UInt32,         typeof(uint),                 0,               0,  0);
        public static readonly ISqlDataType UInt64           = new SqlDataType(DataType.UInt64,         typeof(ulong),                 0, ulong.MaxValue.ToString().Length, 0);
        public static readonly ISqlDataType Single           = DbSingle;
        public static readonly ISqlDataType Double           = DbDouble;
        public static readonly ISqlDataType Decimal          = DbDecimal;
        public static readonly ISqlDataType DateTime         = DbDateTime2;
        public static readonly ISqlDataType String           = DbNVarChar;
        public static readonly ISqlDataType Guid             = DbGuid;
        public static readonly ISqlDataType ByteArray        = DbVarBinary;
        public static readonly ISqlDataType LinqBinary       = DbVarBinary;
        public static readonly ISqlDataType CharArray        = new SqlDataType(DataType.NVarChar,       typeof(char[]),      GetMaxLength,               0,  0);
        public static readonly ISqlDataType DateTimeOffset   = DbDateTimeOffset;
        public static readonly ISqlDataType TimeSpan         = DbTime;

#if !SILVERLIGHT && !NETFX_CORE
        public static readonly ISqlDataType SqlByte          = new SqlDataType(DataType.Byte,           typeof(SqlByte),                0,               0,  0);
        public static readonly ISqlDataType SqlInt16         = new SqlDataType(DataType.Int16,          typeof(SqlInt16),               0,               0,  0);
        public static readonly ISqlDataType SqlInt32         = new SqlDataType(DataType.Int32,          typeof(SqlInt32),               0,               0,  0);
        public static readonly ISqlDataType SqlInt64         = new SqlDataType(DataType.Int64,          typeof(SqlInt64),               0,               0,  0);
        public static readonly ISqlDataType SqlSingle        = new SqlDataType(DataType.Single,         typeof(SqlSingle),              0,               0,  0);
        public static readonly ISqlDataType SqlBoolean       = new SqlDataType(DataType.Boolean,        typeof(SqlBoolean),             0,               0,  0);
        public static readonly ISqlDataType SqlDouble        = new SqlDataType(DataType.Double,         typeof(SqlDouble),              0,               0,  0);
        public static readonly ISqlDataType SqlDateTime      = new SqlDataType(DataType.DateTime,       typeof(SqlDateTime),            0,               0,  0);
        public static readonly ISqlDataType SqlDecimal       = new SqlDataType(DataType.Decimal,        typeof(SqlDecimal),             0, GetMaxPrecision, 10);
        public static readonly ISqlDataType SqlMoney         = new SqlDataType(DataType.Money,          typeof(SqlMoney),               0, GetMaxPrecision,  4);
        public static readonly ISqlDataType SqlString        = new SqlDataType(DataType.NVarChar,       typeof(SqlString),   GetMaxLength,               0,  0);
        public static readonly ISqlDataType SqlBinary        = new SqlDataType(DataType.Binary,         typeof(SqlBinary),   GetMaxLength,               0,  0);
        public static readonly ISqlDataType SqlGuid          = new SqlDataType(DataType.Guid,           typeof(SqlGuid),                0,               0,  0);
        public static readonly ISqlDataType SqlBytes         = new SqlDataType(DataType.Image,          typeof(SqlBytes),    GetMaxLength,               0,  0);
        public static readonly ISqlDataType SqlChars         = new SqlDataType(DataType.Text,           typeof(SqlChars),    GetMaxLength,               0,  0);
        public static readonly ISqlDataType SqlXml           = new SqlDataType(DataType.Xml,            typeof(SqlXml),                 0,               0,  0);
#endif

        #endregion

        #region Overrides

#if OVERRIDETOSTRING

        public override string ToString()
        {
            return ((IQueryElement)this).ToString(new StringBuilder(), new Dictionary<IQueryElement,IQueryElement>()).ToString();
        }

#endif

        #endregion

        #region ISqlExpression Members

        public int Precedence => SqlQuery.Precedence.Primary;

        public Type SystemType => typeof(Type);

        #endregion

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> func)
        {
            return func(this);
        }

        #endregion

        #region IEquatable<ISqlExpression> Members

        bool IEquatable<IQueryExpression>.Equals(IQueryExpression other)
        {
            if (this == other)
                return true;

            var value = (ISqlDataType)other;
            return Type == value.Type && Length == value.Length && Precision == value.Precision && Scale == value.Scale;
        }

        #endregion

        #region ISqlExpression Members

        public bool CanBeNull()
        {
            return false;
        }

        public bool Equals(IQueryExpression other, Func<IQueryExpression,IQueryExpression,bool> comparer)
        {
            return ((IQueryExpression)this).Equals(other) && comparer(this, other);
        }

        #endregion

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new SqlDataType(DataType, Type, Length, Precision, Scale));

            return clone;
        }

        #endregion

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.SqlDataType;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            sb.Append(this.DataType);

            if (Length != 0)
                sb.Append('(').Append(Length).Append(')');
            else if (Precision != 0)
                sb.Append('(').Append(Precision).Append(',').Append(Scale).Append(')');

            return sb;
        }

        #endregion
    }
}
