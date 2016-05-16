using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.SqlClient;
using System.Globalization;
using System.Reflection;
using Bars2Db.Common;
using Bars2Db.Extensions;
using Bars2Db.Linq;
using Bars2Db.SqlQuery;
using Bars2Db.SqlQuery.QueryElements.SqlElements;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlEntities
{
    using PN = ProviderName;

    public static partial class Sql
    {
        #region Guid Functions

        [Function(PN.Oracle, "Sys_Guid", ServerSideOnly = true)]
        [Function(PN.Firebird, "Gen_Uuid", ServerSideOnly = true)]
        [Function(PN.MySql, "Uuid", ServerSideOnly = true)]
        [Expression(PN.Sybase, "NewID(1)", ServerSideOnly = true)]
        [Expression(PN.SapHana, "SYSUUID", ServerSideOnly = true)]
        [Function("NewID", ServerSideOnly = true)]
        public static Guid NewGuid()
        {
            return Guid.NewGuid();
        }

        #endregion

        #region Binary Functions

        [Function(PreferServerSide = true)]
        [Function(PN.Access, "Len", PreferServerSide = true)]
        [Function(PN.Firebird, "Octet_Length", PreferServerSide = true)]
        [Function(PN.SqlServer, "DataLength", PreferServerSide = true)]
        [Function(PN.SqlCe, "DataLength", PreferServerSide = true)]
        [Function(PN.Sybase, "DataLength", PreferServerSide = true)]
        public static int? Length(Binary value)
        {
            return value == null ? null : (int?) value.Length;
        }

        #endregion

        #region Text Functions

        [Expression("FREETEXT({0}, {1})", ServerSideOnly = true)]
        public static bool FreeText(object table, string text)
        {
            throw new LinqException("'FreeText' is only server-side method.");
        }

        #endregion

        #region Common Functions

        [Expression("*", ServerSideOnly = true)]
        public static object[] AllColumns()
        {
            throw new LinqException("'FreeText' is only server-side method.");
        }

        [CLSCompliant(false)]
        [Expression("{0}", 0, ServerSideOnly = true)]
        public static T AsSql<T>(T obj)
        {
            return obj;
        }

        [CLSCompliant(false)]
        [Expression("{0}", 0, ServerSideOnly = true, InlineParameters = true)]
        public static T ToSql<T>(T obj)
        {
            return obj;
        }

        [CLSCompliant(false)]
        [Expression("{0}", 0)]
        public static T? AsNullable<T>(T value)
            where T : struct
        {
            return value;
        }

        [CLSCompliant(false)]
        [Expression("{0}", 0)]
        public static T ConvertNullable<T>(T? value)
            where T : struct
        {
            return value ?? default(T);
        }

        [CLSCompliant(false)]
        [Expression("{0}", 0)]
        public static T AsNotNull<T>(T? value)
            where T : struct
        {
            return value ?? default(T);
        }

        #endregion

        #region Convert Functions

        [CLSCompliant(false)]
        [Function("Convert", 0, 1, ServerSideOnly = true)]
        public static TTo Convert<TTo, TFrom>(TTo to, TFrom from)
        {
            var dt = Common.ConvertTo<TTo>.From(from);
            return dt;
        }

        [CLSCompliant(false)]
        [Function("Convert", 0, 1, 2, ServerSideOnly = true)]
        public static TTo Convert<TTo, TFrom>(TTo to, TFrom from, int format)
        {
            var dt = Common.ConvertTo<TTo>.From(from);
            return dt;
        }

        [CLSCompliant(false)]
        [Function("Convert", 0, 1)]
        public static TTo Convert2<TTo, TFrom>(TTo to, TFrom from)
        {
            return Common.ConvertTo<TTo>.From(from);
        }

        [CLSCompliant(false)]
        [Function("$Convert$", 1, 2, 0)]
        public static TTo Convert<TTo, TFrom>(TFrom obj)
        {
            return Common.ConvertTo<TTo>.From(obj);
        }

        public static class ConvertTo<TTo>
        {
            [CLSCompliant(false)]
            [Function("$Convert$", 1, 2, 0)]
            public static TTo From<TFrom>(TFrom obj)
            {
                return Common.ConvertTo<TTo>.From(obj);
            }
        }

        [Expression("{0}")]
        public static TimeSpan? DateToTime(DateTime? date)
        {
            return date == null ? null : (TimeSpan?) new TimeSpan(date.Value.Ticks);
        }

        [Property(PN.Informix, "Boolean", ServerSideOnly = true)]
        [Property(PN.PostgreSQL, "Boolean", ServerSideOnly = true)]
        [Property(PN.MySql, "Boolean", ServerSideOnly = true)]
        [Property(PN.SQLite, "Boolean", ServerSideOnly = true)]
        [Property(PN.SapHana, "TinyInt", ServerSideOnly = true)]
        [Property("Bit", ServerSideOnly = true)]
        public static bool Bit => false;

        [Property(PN.Oracle, "Number(19)", ServerSideOnly = true)]
        [Property("BigInt", ServerSideOnly = true)]
        public static long BigInt => 0;

        [Property(PN.MySql, "Signed", ServerSideOnly = true)]
        [Property("Int", ServerSideOnly = true)]
        public static int Int => 0;

        [Property(PN.MySql, "Signed", ServerSideOnly = true)]
        [Property("SmallInt", ServerSideOnly = true)]
        public static short SmallInt => 0;

        [Property(PN.DB2, "SmallInt", ServerSideOnly = true)]
        [Property(PN.Informix, "SmallInt", ServerSideOnly = true)]
        [Property(PN.Oracle, "Number(3)", ServerSideOnly = true)]
        [Property(PN.DB2, "SmallInt", ServerSideOnly = true)]
        [Property(PN.Firebird, "SmallInt", ServerSideOnly = true)]
        [Property(PN.PostgreSQL, "SmallInt", ServerSideOnly = true)]
        [Property(PN.MySql, "Unsigned", ServerSideOnly = true)]
        [Property("TinyInt", ServerSideOnly = true)]
        public static byte TinyInt => 0;

        [Property("Decimal", ServerSideOnly = true)]
        public static decimal DefaultDecimal => 0;

        [Expression(PN.SapHana, "Decimal({0},4)", ServerSideOnly = true)]
        [Function(ServerSideOnly = true)]
        public static decimal Decimal(int precision)
        {
            return 0;
        }

        [Function(ServerSideOnly = true)]
        public static decimal Decimal(int precision, int scale)
        {
            return 0;
        }

        [Property(PN.Oracle, "Number(19,4)", ServerSideOnly = true)]
        [Property(PN.Firebird, "Decimal(18,4)", ServerSideOnly = true)]
        [Property(PN.PostgreSQL, "Decimal(19,4)", ServerSideOnly = true)]
        [Property(PN.MySql, "Decimal(19,4)", ServerSideOnly = true)]
        [Property(PN.SapHana, "Decimal(19,4)", ServerSideOnly = true)]
        [Property("Money", ServerSideOnly = true)]
        public static decimal Money => 0;

        [Property(PN.Informix, "Decimal(10,4)", ServerSideOnly = true)]
        [Property(PN.Oracle, "Number(10,4)", ServerSideOnly = true)]
        [Property(PN.Firebird, "Decimal(10,4)", ServerSideOnly = true)]
        [Property(PN.PostgreSQL, "Decimal(10,4)", ServerSideOnly = true)]
        [Property(PN.MySql, "Decimal(10,4)", ServerSideOnly = true)]
        [Property(PN.SqlCe, "Decimal(10,4)", ServerSideOnly = true)]
        [Property(PN.SapHana, "Decimal(10,4)", ServerSideOnly = true)]
        [Property("SmallMoney", ServerSideOnly = true)]
        public static decimal SmallMoney => 0;

        [Property(PN.MySql, "Decimal(29,10)", ServerSideOnly = true)]
        [Property(PN.SapHana, "Double", ServerSideOnly = true)]
        [Property("Float", ServerSideOnly = true)]
        public static double Float => 0;

        [Property(PN.MySql, "Decimal(29,10)", ServerSideOnly = true)]
        [Property("Real", ServerSideOnly = true)]
        public static float Real => 0;

        [Property(PN.PostgreSQL, "TimeStamp", ServerSideOnly = true)]
        [Property(PN.Firebird, "TimeStamp", ServerSideOnly = true)]
        [Property(PN.SapHana, "TimeStamp", ServerSideOnly = true)]
        [Property("DateTime", ServerSideOnly = true)]
        public static DateTime DateTime => DateTime.Now;

        [Property(PN.SqlServer2000, "DateTime", ServerSideOnly = true)]
        [Property(PN.SqlServer2005, "DateTime", ServerSideOnly = true)]
        [Property(PN.PostgreSQL, "TimeStamp", ServerSideOnly = true)]
        [Property(PN.Firebird, "TimeStamp", ServerSideOnly = true)]
        [Property(PN.MySql, "DateTime", ServerSideOnly = true)]
        [Property(PN.SqlCe, "DateTime", ServerSideOnly = true)]
        [Property(PN.Sybase, "DateTime", ServerSideOnly = true)]
        [Property(PN.SapHana, "TimeStamp", ServerSideOnly = true)]
        [Property("DateTime2", ServerSideOnly = true)]
        public static DateTime DateTime2 => DateTime.Now;

        [Property(PN.PostgreSQL, "TimeStamp", ServerSideOnly = true)]
        [Property(PN.Firebird, "TimeStamp", ServerSideOnly = true)]
        [Property(PN.MySql, "DateTime", ServerSideOnly = true)]
        [Property(PN.SqlCe, "DateTime", ServerSideOnly = true)]
        [Property(PN.SapHana, "SecondDate", ServerSideOnly = true)]
        [Property("SmallDateTime", ServerSideOnly = true)]
        public static DateTime SmallDateTime => DateTime.Now;

        [Property(PN.SqlServer2000, "Datetime", ServerSideOnly = true)]
        [Property(PN.SqlServer2005, "Datetime", ServerSideOnly = true)]
        [Property(PN.SqlCe, "Datetime", ServerSideOnly = true)]
        [Property("Date", ServerSideOnly = true)]
        public static DateTime Date => DateTime.Now;

        [Property("Time", ServerSideOnly = true)]
        public static DateTime Time => DateTime.Now;

        [Property(PN.PostgreSQL, "TimeStamp", ServerSideOnly = true)]
        [Property(PN.Firebird, "TimeStamp", ServerSideOnly = true)]
        [Property(PN.SqlServer2012, "DateTimeOffset", ServerSideOnly = true)]
        [Property(PN.SqlServer2008, "DateTimeOffset", ServerSideOnly = true)]
        [Property(PN.SapHana, "TimeStamp", ServerSideOnly = true)]
        [Property("DateTime", ServerSideOnly = true)]
        public static DateTimeOffset DateTimeOffset => DateTimeOffset.Now;

        [Function(PN.SqlCe, "NChar", ServerSideOnly = true)]
        [Function(ServerSideOnly = true)]
        public static string Char(int length)
        {
            return "";
        }

        [Property(PN.SqlCe, "NChar", ServerSideOnly = true)]
        [Property("Char", ServerSideOnly = true)]
        public static string DefaultChar => "";

        [Function(PN.MySql, "Char", ServerSideOnly = true)]
        [Function(PN.SqlCe, "NVarChar", ServerSideOnly = true)]
        [Function(ServerSideOnly = true)]
        public static string VarChar(int length)
        {
            return "";
        }

        [Property(PN.MySql, "Char", ServerSideOnly = true)]
        [Property(PN.SqlCe, "NVarChar", ServerSideOnly = true)]
        [Property("VarChar", ServerSideOnly = true)]
        public static string DefaultVarChar => "";

        [Function(PN.DB2, "Char", ServerSideOnly = true)]
        [Function(ServerSideOnly = true)]
        public static string NChar(int length)
        {
            return "";
        }

        [Property(PN.DB2, "Char", ServerSideOnly = true)]
        [Property("NChar", ServerSideOnly = true)]
        public static string DefaultNChar => "";

        [Function(PN.DB2, "Char", ServerSideOnly = true)]
        [Function(PN.Oracle, "VarChar2", ServerSideOnly = true)]
        [Function(PN.Firebird, "VarChar", ServerSideOnly = true)]
        [Function(PN.PostgreSQL, "VarChar", ServerSideOnly = true)]
        [Function(PN.MySql, "Char", ServerSideOnly = true)]
        [Function(ServerSideOnly = true)]
        public static string NVarChar(int length)
        {
            return "";
        }

        [Property(PN.DB2, "Char", ServerSideOnly = true)]
        [Property(PN.Oracle, "VarChar2", ServerSideOnly = true)]
        [Property(PN.Firebird, "VarChar", ServerSideOnly = true)]
        [Property(PN.PostgreSQL, "VarChar", ServerSideOnly = true)]
        [Property(PN.MySql, "Char", ServerSideOnly = true)]
        [Property("NVarChar", ServerSideOnly = true)]
        public static string DefaultNVarChar => "";

        #endregion

        #region String Functions

        [Function(PreferServerSide = true)]
        [Function(PN.Access, "Len", PreferServerSide = true)]
        [Function(PN.Firebird, "Char_Length", PreferServerSide = true)]
        [Function(PN.SqlServer, "Len", PreferServerSide = true)]
        [Function(PN.SqlCe, "Len", PreferServerSide = true)]
        [Function(PN.Sybase, "Len", PreferServerSide = true)]
        public static int? Length(string str)
        {
            return str == null ? null : (int?) str.Length;
        }

        [Function]
        [Function(PN.Access, "Mid")]
        [Function(PN.DB2, "Substr")]
        [Function(PN.Informix, "Substr")]
        [Function(PN.Oracle, "Substr")]
        [Function(PN.SQLite, "Substr")]
        [Expression(PN.Firebird, "Substring({0} from {1} for {2})")]
        [Function(PN.SapHana, "Substring")]
        public static string Substring(string str, int? startIndex, int? length)
        {
            return str == null || startIndex == null || length == null
                ? null
                : str.Substring(startIndex.Value, length.Value);
        }

        [Function(ServerSideOnly = true)]
        public static bool Like(string matchExpression, string pattern)
        {
#if SILVERLIGHT || NETFX_CORE
            throw new InvalidOperationException();
#else
            return matchExpression != null && pattern != null &&
                   SqlMethods.Like(matchExpression, pattern);
#endif
        }

        [Function(ServerSideOnly = true)]
        public static bool Like(string matchExpression, string pattern, char? escapeCharacter)
        {
#if SILVERLIGHT || NETFX_CORE
            throw new InvalidOperationException();
#else
            return matchExpression != null && pattern != null && escapeCharacter != null &&
                   SqlMethods.Like(matchExpression, pattern, escapeCharacter.Value);
#endif
        }

        [CLSCompliant(false)]
        [Function]
        [Function(PN.DB2, "Locate")]
        [Function(PN.MySql, "Locate")]
        [Function(PN.SapHana, "Locate", 1, 0)]
        public static int? CharIndex(string value, string str)
        {
            if (str == null || value == null)
                return null;

            return str.IndexOf(value) + 1;
        }

        [Function]
        [Function(ProviderName.DB2, "Locate")]
        [Function(ProviderName.MySql, "Locate")]
        [Expression(PN.SapHana, "Locate(Substring({1},{2} + 1),{0}) + {2}")]
        public static int? CharIndex(string value, string str, int? startLocation)
        {
            if (str == null || value == null || startLocation == null)
                return null;

            return str.IndexOf(value, startLocation.Value - 1) + 1;
        }

        [Function]
        [Function(PN.DB2, "Locate")]
        [Function(PN.MySql, "Locate")]
        [Function(PN.SapHana, "Locate")]
        public static int? CharIndex(char? value, string str)
        {
            if (value == null || str == null)
                return null;

            return str.IndexOf(value.Value) + 1;
        }

        [Function]
        [Function(ProviderName.DB2, "Locate")]
        [Function(ProviderName.MySql, "Locate")]
        [Function(PN.SapHana, "Locate")]
        public static int? CharIndex(char? value, string str, int? startLocation)
        {
            if (str == null || value == null || startLocation == null)
                return null;

            return str.IndexOf(value.Value, startLocation.Value - 1) + 1;
        }

        [Function]
        public static string Reverse(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var chars = str.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        [Function(PreferServerSide = true)]
        [Function(PN.SQLite, "LeftStr", PreferServerSide = true)]
        public static string Left(string str, int? length)
        {
            return length == null || str == null || str.Length < length ? null : str.Substring(1, length.Value);
        }

        [Function(PreferServerSide = true)]
        [Function(PN.SQLite, "RightStr", PreferServerSide = true)]
        public static string Right(string str, int? length)
        {
            return length == null || str == null || str.Length < length
                ? null
                : str.Substring(str.Length - length.Value);
        }

        [Function]
        public static string Stuff(string str, int? startLocation, int? length, string value)
        {
            return str == null || value == null || startLocation == null || length == null
                ? null
                : str.Remove(startLocation.Value - 1, length.Value).Insert(startLocation.Value - 1, value);
        }

        [Function(ServerSideOnly = true)]
        public static string Stuff(IEnumerable<string> characterExpression, int? start, int? length,
            string replaceWithExpression)
        {
            throw new NotImplementedException();
        }

        [Function]
        [Expression(ProviderName.SapHana, "Lpad('',{0},' ')")]
        public static string Space(int? length)
        {
            return length == null ? null : "".PadRight(length.Value);
        }

        [Function(Name = "LPad")]
        public static string PadLeft(string str, int? totalWidth, char? paddingChar)
        {
            return str == null || totalWidth == null || paddingChar == null
                ? null
                : str.PadLeft(totalWidth.Value, paddingChar.Value);
        }

        [Function(Name = "RPad")]
        public static string PadRight(string str, int? totalWidth, char? paddingChar)
        {
            return str == null || totalWidth == null || paddingChar == null
                ? null
                : str.PadRight(totalWidth.Value, paddingChar.Value);
        }

        [Function]
        [Function(PN.Sybase, "Str_Replace")]
        public static string Replace(string str, string oldValue, string newValue)
        {
            return str == null || oldValue == null || newValue == null
                ? null
                : str.Replace(oldValue, newValue);
        }

        [Function]
        [Function(PN.Sybase, "Str_Replace")]
        public static string Replace(string str, char? oldValue, char? newValue)
        {
            return str == null || oldValue == null || newValue == null
                ? null
                : str.Replace(oldValue.Value, newValue.Value);
        }

        [Function]
        public static string Trim(string str)
        {
            return str == null ? null : str.Trim();
        }

        [Function("LTrim")]
        public static string TrimLeft(string str)
        {
            return str == null ? null : str.TrimStart();
        }

        [Function("RTrim")]
        public static string TrimRight(string str)
        {
            return str == null ? null : str.TrimEnd();
        }

        [Expression(PN.DB2, "Strip({0}, B, {1})")]
        [Function]
        public static string Trim(string str, char? ch)
        {
            return str == null || ch == null ? null : str.Trim(ch.Value);
        }

        [Expression(PN.DB2, "Strip({0}, L, {1})")]
        [Function("LTrim")]
        public static string TrimLeft(string str, char? ch)
        {
            return str == null || ch == null ? null : str.TrimStart(ch.Value);
        }

        [Expression(PN.DB2, "Strip({0}, T, {1})")]
        [Function("RTrim")]
        public static string TrimRight(string str, char? ch)
        {
            return str == null || ch == null ? null : str.TrimEnd(ch.Value);
        }

        [Function]
        [Function(PN.Access, "LCase")]
        public static string Lower(string str)
        {
            return str == null ? null : str.ToLower();
        }

        [Function]
        [Function(PN.Access, "UCase")]
        public static string Upper(string str)
        {
            return str == null ? null : str.ToUpper();
        }

        private class ConcatAttribute : ExpressionAttribute
        {
            public ConcatAttribute() : base("")
            {
            }

            public override IQueryExpression GetExpression(MemberInfo member, params IQueryExpression[] args)
            {
                var arr = new IQueryExpression[args.Length];

                for (var i = 0; i < args.Length; i++)
                {
                    var arg = args[i];

                    if (arg.SystemType == typeof(string))
                    {
                        arr[i] = arg;
                    }
                    else
                    {
                        var len = arg.SystemType == null || arg.SystemType == typeof(object)
                            ? 100
                            : SqlDataType.GetMaxDisplaySize(SqlDataType.GetDataType(arg.SystemType).DataType);

                        arr[i] = new SqlFunction(typeof(string), "Convert", new SqlDataType(DataType.VarChar, len), arg);
                    }
                }

                if (arr.Length == 1)
                    return arr[0];

                var expr = new SqlBinaryExpression(typeof(string), arr[0], "+", arr[1]);

                for (var i = 2; i < arr.Length; i++)
                    expr = new SqlBinaryExpression(typeof(string), expr, "+", arr[i]);

                return expr;
            }
        }

        [Concat]
        public static string Concat(params object[] args)
        {
            return string.Concat(args);
        }

        [Concat]
        public static string Concat(params string[] args)
        {
            return string.Concat(args);
        }

        #endregion

        #region DateTime Functions

        [Property("CURRENT_TIMESTAMP")]
        [Property(PN.Informix, "CURRENT")]
        [Property(PN.Access, "Now")]
        public static DateTime GetDate()
        {
            return DateTime.Now;
        }

        [Property("CURRENT_TIMESTAMP", ServerSideOnly = true)]
        [Property(PN.Informix, "CURRENT", ServerSideOnly = true)]
        [Property(PN.Access, "Now", ServerSideOnly = true)]
        [Function(PN.SqlCe, "GetDate", ServerSideOnly = true)]
        [Function(PN.Sybase, "GetDate", ServerSideOnly = true)]
        public static DateTime CurrentTimestamp
        {
            get { throw new LinqException("The 'CurrentTimestamp' is server side only property."); }
        }

        [Property("CURRENT_TIMESTAMP")]
        [Property(PN.Informix, "CURRENT")]
        [Property(PN.Access, "Now")]
        [Function(PN.SqlCe, "GetDate")]
        [Function(PN.Sybase, "GetDate")]
        public static DateTime CurrentTimestamp2 => DateTime.Now;

        [Function]
        public static DateTime? ToDate(int? year, int? month, int? day, int? hour, int? minute, int? second,
            int? millisecond)
        {
            return year == null || month == null || day == null || hour == null || minute == null || second == null ||
                   millisecond == null
                ? (DateTime?) null
                : new DateTime(year.Value, month.Value, day.Value, hour.Value, minute.Value, second.Value,
                    millisecond.Value);
        }

        [Function]
        public static DateTime? ToDate(int? year, int? month, int? day, int? hour, int? minute, int? second)
        {
            return year == null || month == null || day == null || hour == null || minute == null || second == null
                ? (DateTime?) null
                : new DateTime(year.Value, month.Value, day.Value, hour.Value, minute.Value, second.Value);
        }

        [Function]
        public static DateTime? ToDate(int? year, int? month, int? day)
        {
            return year == null || month == null || day == null
                ? (DateTime?) null
                : new DateTime(year.Value, month.Value, day.Value);
        }

        [Enum]
        public enum DateParts
        {
            Year = 0,
            Quarter = 1,
            Month = 2,
            DayOfYear = 3,
            Day = 4,
            Week = 5,
            WeekDay = 6,
            Hour = 7,
            Minute = 8,
            Second = 9,
            Millisecond = 10
        }

        private class DatePartAttribute : ExpressionAttribute
        {
            private readonly int _datePartIndex;

            private readonly bool _isExpression;
            private readonly string[] _partMapping;

            public DatePartAttribute(string sqlProvider, string expression, int datePartIndex, params int[] argIndices)
                : this(sqlProvider, expression, SqlQuery.Precedence.Primary, false, null, datePartIndex, argIndices)
            {
            }

            public DatePartAttribute(string sqlProvider, string expression, bool isExpression, int datePartIndex,
                params int[] argIndices)
                : this(
                    sqlProvider, expression, SqlQuery.Precedence.Primary, isExpression, null, datePartIndex, argIndices)
            {
            }

            public DatePartAttribute(string sqlProvider, string expression, bool isExpression, string[] partMapping,
                int datePartIndex, params int[] argIndices)
                : this(
                    sqlProvider, expression, SqlQuery.Precedence.Primary, isExpression, partMapping, datePartIndex,
                    argIndices)
            {
            }

            public DatePartAttribute(string sqlProvider, string expression, int precedence, bool isExpression,
                string[] partMapping, int datePartIndex, params int[] argIndices)
                : base(sqlProvider, expression, argIndices)
            {
                _isExpression = isExpression;
                _partMapping = partMapping;
                _datePartIndex = datePartIndex;
                Precedence = precedence;
            }

            public override IQueryExpression GetExpression(MemberInfo member, params IQueryExpression[] args)
            {
                var part = (DateParts) ((ISqlValue) args[_datePartIndex]).Value;
                var pstr = _partMapping != null ? _partMapping[(int) part] : part.ToString();
                var str = Expression.Args(pstr ?? part.ToString());
                var type = member.GetMemberType();

                return _isExpression
                    ? new SqlExpression(type, str, Precedence, ConvertArgs(member, args))
                    : (IQueryExpression) new SqlFunction(type, str, ConvertArgs(member, args));
            }
        }

        [CLSCompliant(false)]
        [Function] // FIXME: LinqToDB.Sql.DatePartAttribute -> DatePart
        [DatePart(PN.Oracle, "Add{0}", false, 0, 2, 1)]
        [DatePart(PN.DB2, "{{1}} + {0}", Precedence.Additive, true,
            new[]
            {
                "{0} Year", "({0} * 3) Month", "{0} Month", "{0} Day", "{0} Day", "({0} * 7) Day", "{0} Day", "{0} Hour",
                "{0} Minute", "{0} Second", "({0} * 1000) Microsecond"
            }, 0, 1, 2)]
        [DatePart(PN.Informix, "{{1}} + Interval({0}", Precedence.Additive, true,
            new[]
            {
                "{0}) Year to Year", "{0}) Month to Month * 3", "{0}) Month to Month", "{0}) Day to Day", "{0}) Day to Day",
                "{0}) Day to Day * 7", "{0}) Day to Day", "{0}) Hour to Hour", "{0}) Minute to Minute",
                "{0}) Second to Second", null
            }, 0, 1, 2)]
        [DatePart(PN.PostgreSQL, "{{1}} + {{0}} * Interval '1 {0}", Precedence.Additive, true,
            new[]
            {
                "Year'", "Month' * 3", "Month'", "Day'", "Day'", "Day' * 7", "Day'", "Hour'", "Minute'", "Second'",
                "Millisecond'"
            }, 0, 1, 2)]
        [DatePart(PN.MySql, "Date_Add({{1}}, Interval {{0}} {0})", true,
            new[] {null, null, null, "Day", null, null, "Day", null, null, null, null}, 0, 1, 2)]
        [DatePart(PN.SQLite, "DateTime({{1}}, '{{0}} {0}')", true,
            new[] {null, null, null, "Day", null, null, "Day", null, null, null, null}, 0, 1, 2)]
        [DatePart(PN.Access, "DateAdd({0}, {{0}}, {{1}})", true,
            new[] {"'yyyy'", "'q'", "'m'", "'y'", "'d'", "'ww'", "'w'", "'h'", "'n'", "'s'", null}, 0, 1, 2)]
        [DatePart(PN.SapHana, "Add_{0}", true,
            new[]
            {
                "Years({1}, {0})", "Months({1}, {0} * 3)", "Months({1}, {0})", "Days({1}, {0})", "Days({1}, {0})",
                "Days({1}, {0} * 7)", "Days({1}, {0})", "Seconds({1}, {0} * 3600)", "Seconds({1}, {0} * 60)",
                "Seconds({1}, {0})", null
            }, 0, 1, 2)]
        public static DateTime? DateAdd(DateParts part, double? number, DateTime? date)
        {
            if (number == null || date == null)
                return null;

            switch (part)
            {
                case DateParts.Year:
                    return date.Value.AddYears((int) number);
                case DateParts.Quarter:
                    return date.Value.AddMonths((int) number*3);
                case DateParts.Month:
                    return date.Value.AddMonths((int) number);
                case DateParts.DayOfYear:
                    return date.Value.AddDays(number.Value);
                case DateParts.Day:
                    return date.Value.AddDays(number.Value);
                case DateParts.Week:
                    return date.Value.AddDays(number.Value*7);
                case DateParts.WeekDay:
                    return date.Value.AddDays(number.Value);
                case DateParts.Hour:
                    return date.Value.AddHours(number.Value);
                case DateParts.Minute:
                    return date.Value.AddMinutes(number.Value);
                case DateParts.Second:
                    return date.Value.AddSeconds(number.Value);
                case DateParts.Millisecond:
                    return date.Value.AddMilliseconds(number.Value);
            }

            throw new InvalidOperationException();
        }

        [CLSCompliant(false)]
        [Function]
        [DatePart(PN.DB2, "{0}", false, new[] {null, null, null, null, null, null, "DayOfWeek", null, null, null, null},
            0, 1)]
        [DatePart(PN.Informix, "{0}", 0, 1)]
        [DatePart(PN.MySql, "Extract({0} from {{0}})", true, 0, 1)]
        [DatePart(PN.PostgreSQL, "Extract({0} from {{0}})", true,
            new[] {null, null, null, "DOY", null, null, "DOW", null, null, null, null}, 0, 1)]
        [DatePart(PN.Firebird, "Extract({0} from {{0}})", true,
            new[] {null, null, null, "YearDay", null, null, null, null, null, null, null}, 0, 1)]
        [DatePart(PN.Oracle, "To_Number(To_Char({{0}}, {0}))", true,
            new[] {"'YYYY'", "'Q'", "'MM'", "'DDD'", "'DD'", "'WW'", "'D'", "'HH24'", "'MI'", "'SS'", "'FF'"}, 0, 1)]
        [DatePart(PN.SQLite, "Cast(StrFTime({0}, {{0}}) as int)", true,
            new[] {"'%Y'", null, "'%m'", "'%j'", "'%d'", "'%W'", "'%w'", "'%H'", "'%M'", "'%S'", "'%f'"}, 0, 1)]
        [DatePart(PN.Access, "DatePart({0}, {{0}})", true,
            new[] {"'yyyy'", "'q'", "'m'", "'y'", "'d'", "'ww'", "'w'", "'h'", "'n'", "'s'", null}, 0, 1)]
        [DatePart(PN.SapHana, "{0}", true,
            new[]
            {
                "Year({0})", "Floor((Month({0})-1) / 3) + 1", "Month({0})", "DayOfYear({0})", "DayOfMonth({0})",
                "Week({0})", "MOD(Weekday({0}) + 1, 7) + 1", "Hour({0})", "Minute({0})", "Second({0})", null
            }, 0, 1)]
        public static int? DatePart(DateParts part, DateTime? date)
        {
            if (date == null)
                return null;

            switch (part)
            {
                case DateParts.Year:
                    return date.Value.Year;
                case DateParts.Quarter:
                    return (date.Value.Month - 1)/3 + 1;
                case DateParts.Month:
                    return date.Value.Month;
                case DateParts.DayOfYear:
                    return date.Value.DayOfYear;
                case DateParts.Day:
                    return date.Value.Day;
                case DateParts.Week:
                    return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date.Value, CalendarWeekRule.FirstDay,
                        DayOfWeek.Sunday);
                case DateParts.WeekDay:
                    return ((int) date.Value.DayOfWeek + 1 + DateFirst + 6)%7 + 1;
                case DateParts.Hour:
                    return date.Value.Hour;
                case DateParts.Minute:
                    return date.Value.Minute;
                case DateParts.Second:
                    return date.Value.Second;
                case DateParts.Millisecond:
                    return date.Value.Millisecond;
            }

            throw new InvalidOperationException();
        }

        [CLSCompliant(false)]
        [Function]
        [Function(PN.MySql, "TIMESTAMPDIFF")]
        [DatePart(PN.SapHana, "{0}", true,
            new[]
            {
                null, null, null, null, "Days_Between({0}, {1})", null, null, "Seconds_Between({0}, {1}) / 3600",
                "Seconds_Between({0}, {1}) / 60", "Seconds_Between({0}, {1})", "Nano100_Between({0}, {1}) / 10000"
            }, 0,
            1, 2)]
        public static int? DateDiff(DateParts part, DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null || endDate == null)
                return null;

            switch (part)
            {
                case DateParts.Day:
                    return (int) (endDate - startDate).Value.TotalDays;
                case DateParts.Hour:
                    return (int) (endDate - startDate).Value.TotalHours;
                case DateParts.Minute:
                    return (int) (endDate - startDate).Value.TotalMinutes;
                case DateParts.Second:
                    return (int) (endDate - startDate).Value.TotalSeconds;
                case DateParts.Millisecond:
                    return (int) (endDate - startDate).Value.TotalMilliseconds;
            }

            throw new InvalidOperationException();
        }

        [Property("@@DATEFIRST")]
        public static int DateFirst => 7;

        [Function]
        public static DateTime? MakeDateTime(int? year, int? month, int? day)
        {
            return year == null || month == null || day == null
                ? (DateTime?) null
                : new DateTime(year.Value, month.Value, day.Value);
        }

        [Function]
        public static DateTime? MakeDateTime(int? year, int? month, int? day, int? hour, int? minute, int? second)
        {
            return year == null || month == null || day == null || hour == null || minute == null || second == null
                ? (DateTime?) null
                : new DateTime(year.Value, month.Value, day.Value, hour.Value, minute.Value, second.Value);
        }

        #endregion

        #region Math Functions

        [Function]
        public static decimal? Abs(decimal? value)
        {
            return value == null ? null : (decimal?) Math.Abs(value.Value);
        }

        [Function]
        public static double? Abs(double? value)
        {
            return value == null ? null : (double?) Math.Abs(value.Value);
        }

        [Function]
        public static short? Abs(short? value)
        {
            return value == null ? null : (short?) Math.Abs(value.Value);
        }

        [Function]
        public static int? Abs(int? value)
        {
            return value == null ? null : (int?) Math.Abs(value.Value);
        }

        [Function]
        public static long? Abs(long? value)
        {
            return value == null ? null : (long?) Math.Abs(value.Value);
        }

        [CLSCompliant(false)]
        [Function]
        public static sbyte? Abs(sbyte? value)
        {
            return value == null ? null : (sbyte?) Math.Abs(value.Value);
        }

        [Function]
        public static float? Abs(float? value)
        {
            return value == null ? null : (float?) Math.Abs(value.Value);
        }

        [Function]
        public static double? Acos(double? value)
        {
            return value == null ? null : (double?) Math.Acos(value.Value);
        }

        [Function]
        public static double? Asin(double? value)
        {
            return value == null ? null : (double?) Math.Asin(value.Value);
        }

        [Function(PN.Access, "Atn")]
        [Function]
        public static double? Atan(double? value)
        {
            return value == null ? null : (double?) Math.Atan(value.Value);
        }

        [CLSCompliant(false)]
        [Function(PN.SqlServer, "Atn2")]
        [Function(PN.DB2, "Atan2", 1, 0)]
        [Function(PN.SqlCe, "Atn2")]
        [Function(PN.Sybase, "Atn2")]
        [Function]
        public static double? Atan2(double? x, double? y)
        {
            return x == null || y == null ? null : (double?) Math.Atan2(x.Value, y.Value);
        }

        [Function(PN.Informix, "Ceil")]
        [Function(PN.Oracle, "Ceil")]
        [Function(PN.SapHana, "Ceil")]
        [Function]
        public static decimal? Ceiling(decimal? value)
        {
            return value == null ? null : (decimal?) decimal.Ceiling(value.Value);
        }

        [Function(PN.Informix, "Ceil")]
        [Function(PN.Oracle, "Ceil")]
        [Function(PN.SapHana, "Ceil")]
        [Function]
        public static double? Ceiling(double? value)
        {
            return value == null ? null : (double?) Math.Ceiling(value.Value);
        }

        [Function]
        public static double? Cos(double? value)
        {
            return value == null ? null : (double?) Math.Cos(value.Value);
        }

        [Function]
        public static double? Cosh(double? value)
        {
            return value == null ? null : (double?) Math.Cosh(value.Value);
        }

        [Function]
        public static double? Cot(double? value)
        {
            return value == null ? null : (double?) Math.Cos(value.Value)/Math.Sin(value.Value);
        }

        [Function]
        public static decimal? Degrees(decimal? value)
        {
            return value == null ? null : (decimal?) (value.Value*180m/(decimal) Math.PI);
        }

        [Function]
        public static double? Degrees(double? value)
        {
            return value == null ? null : (double?) (value.Value*180/Math.PI);
        }

        [Function]
        public static short? Degrees(short? value)
        {
            return value == null ? null : (short?) (value.Value*180/Math.PI);
        }

        [Function]
        public static int? Degrees(int? value)
        {
            return value == null ? null : (int?) (value.Value*180/Math.PI);
        }

        [Function]
        public static long? Degrees(long? value)
        {
            return value == null ? null : (long?) (value.Value*180/Math.PI);
        }

        [CLSCompliant(false)]
        [Function]
        public static sbyte? Degrees(sbyte? value)
        {
            return value == null ? null : (sbyte?) (value.Value*180/Math.PI);
        }

        [Function]
        public static float? Degrees(float? value)
        {
            return value == null ? null : (float?) (value.Value*180/Math.PI);
        }

        [Function]
        public static double? Exp(double? value)
        {
            return value == null ? null : (double?) Math.Exp(value.Value);
        }

        [Function(PN.Access, "Int")]
        [Function]
        public static decimal? Floor(decimal? value)
        {
            return value == null ? null : (decimal?) decimal.Floor(value.Value);
        }

        [Function(PN.Access, "Int")]
        [Function]
        public static double? Floor(double? value)
        {
            return value == null ? null : (double?) Math.Floor(value.Value);
        }

        [Function(PN.Informix, "LogN")]
        [Function(PN.Oracle, "Ln")]
        [Function(PN.Firebird, "Ln")]
        [Function(PN.PostgreSQL, "Ln")]
        [Function(PN.SapHana, "Ln")]
        [Function]
        public static decimal? Log(decimal? value)
        {
            return value == null ? null : (decimal?) Math.Log((double) value.Value);
        }

        [Function(PN.Informix, "LogN")]
        [Function(PN.Oracle, "Ln")]
        [Function(PN.Firebird, "Ln")]
        [Function(PN.PostgreSQL, "Ln")]
        [Function(PN.SapHana, "Ln")]
        [Function]
        public static double? Log(double? value)
        {
            return value == null ? null : (double?) Math.Log(value.Value);
        }

        [Function(PN.PostgreSQL, "Log")]
        [Expression(PN.SapHana, "Log(10,{0})")]
        [Function]
        public static double? Log10(double? value)
        {
            return value == null ? null : (double?) Math.Log10(value.Value);
        }

        [Function]
        public static double? Log(double? newBase, double? value)
        {
            return value == null || newBase == null ? null : (double?) Math.Log(value.Value, newBase.Value);
        }

        [Function]
        public static decimal? Log(decimal? newBase, decimal? value)
        {
            return value == null || newBase == null
                ? null
                : (decimal?) Math.Log((double) value.Value, (double) newBase.Value);
        }

        [Expression(PN.Access, "{0} ^ {1}", Precedence = Precedence.Multiplicative)]
        [Function]
        public static double? Power(double? x, double? y)
        {
            return x == null || y == null ? null : (double?) Math.Pow(x.Value, y.Value);
        }

        [Function]
        public static decimal? RoundToEven(decimal? value)
        {
#if SILVERLIGHT
            return value == null ? null : (Decimal?)Math.Round(value.Value);
#else
            return value == null ? null : (decimal?) Math.Round(value.Value, MidpointRounding.ToEven);
#endif
        }

        [Function]
        public static double? RoundToEven(double? value)
        {
#if SILVERLIGHT
            return value == null ? null : (Double?) Math.Round(value.Value);
#else
            return value == null ? null : (double?) Math.Round(value.Value, MidpointRounding.ToEven);
#endif
        }

        [Function]
        public static decimal? Round(decimal? value)
        {
            return Round(value, 0);
        }

        [Function]
        public static double? Round(double? value)
        {
            return Round(value, 0);
        }

        [Function]
        public static decimal? Round(decimal? value, int? precision)
        {
#if SILVERLIGHT
            throw new NotImplementedException();
#else
            return value == null || precision == null
                ? null
                : (decimal?) Math.Round(value.Value, precision.Value, MidpointRounding.AwayFromZero);
#endif
        }

        [Function]
        public static double? Round(double? value, int? precision)
        {
#if SILVERLIGHT
            throw new NotImplementedException();
#else
            return value == null || precision == null
                ? null
                : (double?) Math.Round(value.Value, precision.Value, MidpointRounding.AwayFromZero);
#endif
        }

        [Function]
        public static decimal? RoundToEven(decimal? value, int? precision)
        {
#if SILVERLIGHT
            return value == null || precision == null? null : (Decimal?)Math.Round(value.Value, precision.Value);
#else
            return value == null || precision == null
                ? null
                : (decimal?) Math.Round(value.Value, precision.Value, MidpointRounding.ToEven);
#endif
        }

        [Function]
        public static double? RoundToEven(double? value, int? precision)
        {
#if SILVERLIGHT
            return value == null || precision == null? null : (Double?) Math.Round(value.Value, precision.Value);
#else
            return value == null || precision == null
                ? null
                : (double?) Math.Round(value.Value, precision.Value, MidpointRounding.ToEven);
#endif
        }

        [Function(PN.Access, "Sgn"), Function]
        public static int? Sign(decimal? value)
        {
            return value == null ? null : (int?) Math.Sign(value.Value);
        }

        [Function(PN.Access, "Sgn"), Function]
        public static int? Sign(double? value)
        {
            return value == null ? null : (int?) Math.Sign(value.Value);
        }

        [Function(PN.Access, "Sgn"), Function]
        public static int? Sign(short? value)
        {
            return value == null ? null : (int?) Math.Sign(value.Value);
        }

        [Function(PN.Access, "Sgn"), Function]
        public static int? Sign(int? value)
        {
            return value == null ? null : (int?) Math.Sign(value.Value);
        }

        [Function(PN.Access, "Sgn"), Function]
        public static int? Sign(long? value)
        {
            return value == null ? null : (int?) Math.Sign(value.Value);
        }

        [CLSCompliant(false)]
        [Function(PN.Access, "Sgn"), Function]
        public static int? Sign(sbyte? value)
        {
            return value == null ? null : (int?) Math.Sign(value.Value);
        }

        [Function(PN.Access, "Sgn"), Function]
        public static int? Sign(float? value)
        {
            return value == null ? null : (int?) Math.Sign(value.Value);
        }

        [Function]
        public static double? Sin(double? value)
        {
            return value == null ? null : (double?) Math.Sin(value.Value);
        }

        [Function]
        public static double? Sinh(double? value)
        {
            return value == null ? null : (double?) Math.Sinh(value.Value);
        }

        [Function(PN.Access, "Sqr")]
        [Function]
        public static double? Sqrt(double? value)
        {
            return value == null ? null : (double?) Math.Sqrt(value.Value);
        }

        [Function]
        public static double? Tan(double? value)
        {
            return value == null ? null : (double?) Math.Tan(value.Value);
        }

        [Function]
        public static double? Tanh(double? value)
        {
            return value == null ? null : (double?) Math.Tanh(value.Value);
        }

        [Expression(PN.SqlServer, "Round({0}, 0, 1)")]
        [Expression(PN.DB2, "Truncate({0}, 0)")]
        [Expression(PN.Informix, "Trunc({0}, 0)")]
        [Expression(PN.Oracle, "Trunc({0}, 0)")]
        [Expression(PN.Firebird, "Trunc({0}, 0)")]
        [Expression(PN.PostgreSQL, "Trunc({0}, 0)")]
        [Expression(PN.MySql, "Truncate({0}, 0)")]
        [Expression(PN.SqlCe, "Round({0}, 0, 1)")]
        [Expression(PN.SapHana, "Round({0}, 0, ROUND_DOWN)")]
        [Function]
        public static decimal? Truncate(decimal? value)
        {
#if SILVERLIGHT
            throw new NotImplementedException();
#else
            return value == null ? null : (decimal?) decimal.Truncate(value.Value);
#endif
        }

        [Expression(PN.SqlServer, "Round({0}, 0, 1)")]
        [Expression(PN.DB2, "Truncate({0}, 0)")]
        [Expression(PN.Informix, "Trunc({0}, 0)")]
        [Expression(PN.Oracle, "Trunc({0}, 0)")]
        [Expression(PN.Firebird, "Trunc({0}, 0)")]
        [Expression(PN.PostgreSQL, "Trunc({0}, 0)")]
        [Expression(PN.MySql, "Truncate({0}, 0)")]
        [Expression(PN.SqlCe, "Round({0}, 0, 1)")]
        [Expression(PN.SapHana, "Round({0}, 0, ROUND_DOWN)")]
        [Function]
        public static double? Truncate(double? value)
        {
#if SILVERLIGHT
            throw new NotImplementedException();
#else
            return value == null ? null : (double?) Math.Truncate(value.Value);
#endif
        }

        #endregion
    }
}