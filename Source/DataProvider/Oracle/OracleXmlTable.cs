using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LinqToDB.DataProvider.Oracle
{
	using Common;
	using Expressions;

	using LinqToDB.SqlEntities;
	using LinqToDB.SqlQuery.QueryElements.SqlElements;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Enums;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

	using Mapping;

    public static partial class OracleTools
	{
		class OracleXmlTableAttribute : Sql.TableExpressionAttribute
		{
			public OracleXmlTableAttribute()
				: base("")
			{
			}

			static string GetDataTypeText(ISqlDataType type)
			{
				switch (type.DataType)
				{
					case DataType.DateTime   : return "timestamp";
					case DataType.DateTime2  : return "timestamp";
					case DataType.UInt32     :
					case DataType.Int64      : return "Number(19)";
					case DataType.SByte      :
					case DataType.Byte       : return "Number(3)";
					case DataType.Money      : return "Number(19,4)";
					case DataType.SmallMoney : return "Number(10,4)";
					case DataType.NVarChar   : return "VarChar2(" + (type.Length ?? 100) + ")";
					case DataType.NChar      : return "Char2(" + (type.Length ?? 100) + ")";
					case DataType.Double     : return "Float";
					case DataType.Single     : return "Real";
					case DataType.UInt16     : return "Int";
					case DataType.UInt64     : return "Decimal";
					case DataType.Int16      : return "SmallInt";
					case DataType.Int32      : return "Int";
					case DataType.Boolean    : return "Bit";
				}

				var text = type.DataType.ToString();

				if (type.Length > 0)
					text += "(" + type.Length + ")";
				else if (type.Precision > 0)
					text += "(" + type.Precision + "," + type.Scale + ")";

				return text;
			}

			static string ValueConverter(List<Action<StringBuilder,object>> converters, object obj)
			{
				var sb = new StringBuilder("<t>").AppendLine();

				foreach (var item in (IEnumerable)obj)
				{
					sb.Append("<r>");

					for (var i = 0; i < converters.Count; i++)
					{
						sb.Append("<c" + i + ">");
						converters[i](sb, item);
						sb.Append("</c" + i + ">");
					}

					sb.AppendLine("</r>");
				}

				return sb.AppendLine("</t>").ToString();
			}

			internal static Func<object,string> GetXmlConverter(MappingSchema mappingSchema, ISqlTable sqlTable)
			{
				var ed  = mappingSchema.GetEntityDescriptor(sqlTable.ObjectType);

				return o => ValueConverter(
					ed.Columns.Select<ColumnDescriptor,Action<StringBuilder,object>>(c =>
					{
						var conv = mappingSchema.ValueToSqlConverter;
						return (sb,obj) =>
						{
							var value = c.GetValue(obj);

							if (value is string && c.MemberType == typeof(string))
							{
								var str = conv.Convert(new StringBuilder(), value).ToString();

								if (str.Length> 2)
								{
									str = str.Substring(1);
									str = str.Substring(0, str.Length - 1);
									sb.Append(str);
								}
							}
							else
							{
								conv.Convert(sb, value);
							}
						};
					}).ToList(),
					o);
			}

			public override void SetTable(MappingSchema mappingSchema, ISqlTable table, MemberInfo member, IEnumerable<Expression> expArgs, IEnumerable<IQueryExpression> sqlArgs)
			{
				var arg = sqlArgs.ElementAt(1);
				var ed  = mappingSchema.GetEntityDescriptor(table.ObjectType);

			    var sqlParameter = arg as ISqlParameter;
			    if (sqlParameter != null)
				{
					var exp = expArgs.ElementAt(1).Unwrap();

				    var constantExpression = exp as ConstantExpression;
				    if (constantExpression != null)
					{
						if (constantExpression.Value is Func<string>)
						{
							sqlParameter.ValueConverter = l => ((Func<string>)l)();
						}
						else
						{
							sqlParameter.ValueConverter = GetXmlConverter(mappingSchema, table);
						}
					}
					else if (exp is LambdaExpression)
					{
						sqlParameter.ValueConverter = l => ((Func<string>)l)();
					}
				}

				var columns = ed.Columns
					.Select((c,i) => "{0} {1} path 'c{2}'".Args(
						c.ColumnName,
						string.IsNullOrEmpty(c.DbType) ?
							GetDataTypeText(
								new SqlDataType(
									c.DataType == DataType.Undefined ? SqlDataType.GetDataType(c.MemberType).DataType : c.DataType,
									c.MemberType,
									c.Length,
									c.Precision,
									c.Scale)) :
							c.DbType,
						i))
					.Aggregate((s1,s2) => s1 + ", " +  s2);

				table.SqlTableType   = ESqlTableType.Expression;
				table.Name           = "XmlTable('/t/r' PASSING XmlType({2}) COLUMNS " + columns + ") {1}";
                table.TableArguments.Clear();
			    table.TableArguments.AddFirst(arg);
			}
		}

		public static string GetXmlData<T>(MappingSchema mappingSchema, IEnumerable<T> data)
		{
			var sqlTable = new SqlTable(mappingSchema, typeof(T));
			return GetXmlData(mappingSchema, sqlTable, data);
		}

		static string GetXmlData<T>(MappingSchema mappingSchema, ISqlTable sqlTable, IEnumerable<T> data)
		{
			var converter  = OracleXmlTableAttribute.GetXmlConverter(mappingSchema, sqlTable);
			return converter(data);
		}

		[OracleXmlTable]
		public static ITable<T> OracleXmlTable<T>(this IDataContext dataContext, IEnumerable<T> data)
			where T : class
		{
			return dataContext.GetTable<T>(
				null,
				((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
				dataContext,
				data);
		}

		[OracleXmlTable]
		public static ITable<T> OracleXmlTable<T>(this IDataContext dataContext, string xmlData)
			where T : class
		{
			return dataContext.GetTable<T>(
				null,
				((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
				dataContext,
				xmlData);
		}

		[OracleXmlTable]
		public static ITable<T> OracleXmlTable<T>(this IDataContext dataContext, Func<string> xmlData)
			where T : class
		{
			return dataContext.GetTable<T>(
				null,
				((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
				dataContext,
				xmlData);
		}
	}
}
