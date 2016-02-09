using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LinqToDB.ServiceModel
{
	using Common;
	using Extensions;

	using LinqToDB.SqlQuery.QueryElements;
	using LinqToDB.SqlQuery.QueryElements.Clauses;
	using LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces;
	using LinqToDB.SqlQuery.QueryElements.Conditions;
	using LinqToDB.SqlQuery.QueryElements.Enums;
	using LinqToDB.SqlQuery.QueryElements.Interfaces;
	using LinqToDB.SqlQuery.QueryElements.Predicates;
	using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
	using LinqToDB.SqlQuery.QueryElements.SqlElements;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

	using Mapping;
	using SqlQuery;

	static class LinqServiceSerializer
	{
		#region Public Members

		public static string Serialize(ISelectQuery query, SqlParameter[] parameters, List<string> queryHints)
		{
			return new QuerySerializer().Serialize(query, parameters, queryHints);
		}

		public static LinqServiceQuery Deserialize(string str)
		{
			return new QueryDeserializer().Deserialize(str);
		}

		public static string Serialize(LinqServiceResult result)
		{
			return new ResultSerializer().Serialize(result);
		}

		public static LinqServiceResult DeserializeResult(string str)
		{
			return new ResultDeserializer().DeserializeResult(str);
		}

		public static string Serialize(string[] data)
		{
			return new StringArraySerializer().Serialize(data);
		}

		public static string[] DeserializeStringArray(string str)
		{
			return new StringArrayDeserializer().Deserialize(str);
		}

		#endregion

		#region SerializerBase

		const int ParamIndex     = -1;
		const int TypeIndex      = -2;
		const int TypeArrayIndex = -3;

		class SerializerBase
		{
			protected readonly StringBuilder          Builder = new StringBuilder();
			protected readonly Dictionary<object,int> Dic     = new Dictionary<object,int>();
			protected int                             Index;

			static string ConvertToString(Type type, object value)
			{
				switch (type.GetTypeCodeEx())
				{
					case TypeCode.Decimal  : return ((decimal) value).ToString(CultureInfo.InvariantCulture);
					case TypeCode.Double   : return ((double)  value).ToString(CultureInfo.InvariantCulture);
					case TypeCode.Single   : return ((float)   value).ToString(CultureInfo.InvariantCulture);
					case TypeCode.DateTime : return ((DateTime)value).ToString("o");
				}

				if (type == typeof(DateTimeOffset))
					return ((DateTimeOffset)value).ToString("o");

				return Converter.ChangeTypeTo<string>(value);
			}

			protected void Append(Type type, object value)
			{
				Append(type);

				if (value == null)
					Append((string)null);
				else if (!type.IsArray)
				{
					Append(ConvertToString(type, value));
				}
				else
				{
					var elementType = type.GetElementType();

					Builder.Append(' ');

					var len = Builder.Length;
					var cnt = 0;

					if (elementType.IsArray)
						foreach (var val in (IEnumerable)value)
						{
							Append(elementType, val);
							cnt++;
						}
					else
						foreach (var val in (IEnumerable)value)
						{
							if (val == null)
								Append((string)null);
							else
								Append(ConvertToString(val.GetType(), val));
							cnt++;
						}

					Builder.Insert(len, cnt.ToString(CultureInfo.CurrentCulture));
				}
			}

			protected void Append(int value)
			{
				Builder.Append(' ').Append(value);
			}

			protected void Append(int? value)
			{
				Builder.Append(' ').Append(value.HasValue ? '1' : '0');

				if (value.HasValue)
					Builder.Append(value.Value);
			}

			protected void Append(Type value)
			{
				Builder.Append(' ').Append(value == null ? 0 : GetType(value));
			}

			protected void Append(bool value)
			{
				Builder.Append(' ').Append(value ? '1' : '0');
			}

			protected void Append(IQueryElement element)
			{
				Builder.Append(' ').Append(element == null ? 0 : Dic[element]);
			}

			protected void Append(string str)
			{
				Builder.Append(' ');

				if (str == null)
				{
					Builder.Append('-');
				}
				else if (str.Length == 0)
				{
					Builder.Append('0');
				}
				else
				{
					Builder
						.Append(str.Length)
						.Append(':')
						.Append(str);
				}
			}

			protected int GetType(Type type)
			{
				if (type == null)
					return 0;

				int idx;

				if (!Dic.TryGetValue(type, out idx))
				{
					if (type.IsArray)
					{
						var elementType = GetType(type.GetElementType());

						Dic.Add(type, idx = ++Index);

						Builder
							.Append(idx)
							.Append(' ')
							.Append(TypeArrayIndex)
							.Append(' ')
							.Append(elementType);
					}
					else
					{
						Dic.Add(type, idx = ++Index);

						Builder
							.Append(idx)
							.Append(' ')
							.Append(TypeIndex);

						Append(type.FullName);
					}

					Builder.AppendLine();
				}

				return idx;
			}
		}

		#endregion

		#region DeserializerBase

		public class DeserializerBase
		{
			protected readonly Dictionary<int,object> Dic = new Dictionary<int,object>();

			protected string Str;
			protected int    Pos;

			protected char Peek()
			{
				return Str[Pos];
			}

			char Next()
			{
				return Str[++Pos];
			}

			protected bool Get(char c)
			{
				if (Peek() == c)
				{
					Pos++;
					return true;
				}

				return false;
			}

			protected int ReadInt()
			{
				Get(' ');

				var minus = Get('-');
				var value = 0;

				for (var c = Peek(); char.IsDigit(c); c = Next())
					value = value * 10 + (c - '0');

				return minus ? -value : value;
			}

			protected int? ReadNullableInt()
			{
				Get(' ');

				if (Get('0'))
					return null;

				Get('1');

				var minus = Get('-');
				var value = 0;

				for (var c = Peek(); char.IsDigit(c); c = Next())
					value = value * 10 + (c - '0');

				return minus ? -value : value;
			}

			protected int? ReadCount()
			{
				Get(' ');

				if (Get('-'))
					return null;

				var value = 0;

				for (var c = Peek(); char.IsDigit(c); c = Next())
					value = value * 10 + (c - '0');

				return value;
			}

			protected string ReadString()
			{
				Get(' ');

				var c = Peek();

				if (c == '-')
				{
					Pos++;
					return null;
				}

				if (c == '0')
				{
					Pos++;
					return string.Empty;
				}

				var len   = ReadInt();
				var value = Str.Substring(++Pos, len);

				Pos += len;

				return value;
			}

			protected bool ReadBool()
			{
				Get(' ');

				var value = Peek() == '1';

				Pos++;

				return value;
			}

			protected T Read<T>()
				where T : class
			{
				var idx = ReadInt();
				return idx == 0 ? null : (T)Dic[idx];
			}

			protected T[] ReadArray<T>()
				where T : class
			{
				var count = ReadCount();

				if (count == null)
					return null;

				var items = new T[count.Value];

				for (var i = 0; i < count; i++)
					items[i] = Read<T>();

				return items;
			}

			protected List<T> ReadList<T>()
				where T : class
			{
				var count = ReadCount();

				if (count == null)
					return null;

				var items = new List<T>(count.Value);

				for (var i = 0; i < count; i++)
					items.Add(Read<T>());

				return items;
			}

			protected void NextLine()
			{
				while (Pos < Str.Length && (Peek() == '\n' || Peek() == '\r'))
					Pos++;
			}

			interface IDeserializerHelper
			{
				object GetArray(DeserializerBase deserializer);
			}

			class DeserializerHelper<T> : IDeserializerHelper
			{
				public object GetArray(DeserializerBase deserializer)
				{
					var count = deserializer.ReadCount();

					if (count == null)
						return null;

					var arr   = new T[count.Value];
					var type  = typeof(T);

					for (var i = 0; i < count.Value; i++)
						arr[i] = (T)deserializer.ReadValue(type);

					return arr;
				}
			}

			static readonly Dictionary<Type,Func<DeserializerBase,object>> _arrayDeserializers =
				new Dictionary<Type,Func<DeserializerBase,object>>();

			protected object ReadValue(Type type)
			{
				if (type == null)
					return ReadString();

				if (type.IsArray)
				{
					var elem = type.GetElementType();

					Func<DeserializerBase,object> deserializer;

					lock (_arrayDeserializers)
					{
						if (!_arrayDeserializers.TryGetValue(elem, out deserializer))
						{
							var helper = (IDeserializerHelper)Activator.CreateInstance(typeof(DeserializerHelper<>).MakeGenericType(elem));
							_arrayDeserializers.Add(elem, deserializer = helper.GetArray);
						}
					}

					return deserializer(this);
				}

				var str = ReadString();

				if (str == null)
					return null;

				switch (type.GetTypeCodeEx())
				{
					case TypeCode.Decimal  : return decimal. Parse(str, CultureInfo.InvariantCulture);
					case TypeCode.Double   : return double.  Parse(str, CultureInfo.InvariantCulture);
					case TypeCode.Single   : return float.   Parse(str, CultureInfo.InvariantCulture);
					case TypeCode.DateTime : return DateTime.ParseExact(str, "o", CultureInfo.InvariantCulture);
				}

				if (type == typeof(DateTimeOffset))
					return DateTimeOffset.ParseExact(str, "o", CultureInfo.InvariantCulture);

				return Converter.ChangeType(str, type);
			}

			protected readonly List<string> UnresolvedTypes = new List<string>();

			protected Type ResolveType(string str)
			{
				if (str == null)
					return null;

				var type = Type.GetType(str, false);

				if (type == null)
				{
					if (str == "System.Data.Linq.Binary")
						return typeof(System.Data.Linq.Binary);

#if !SILVERLIGHT && !NETFX_CORE

					type = LinqService.TypeResolver(str);

#endif

					if (type == null)
					{
						UnresolvedTypes.Add(str);

						Debug.WriteLine(
							"Type '{0}' cannot be resolved. Use LinqService.TypeResolver to resolve unknown types.".Args(str),
							"LinqServiceSerializer");
					}
				}

				return type;
			}
		}

		#endregion

		#region QuerySerializer

		class QuerySerializer : SerializerBase
		{
			public string Serialize(ISelectQuery query, SqlParameter[] parameters, List<string> queryHints)
			{
				var queryHintCount = queryHints == null ? 0 : queryHints.Count;

				Builder.AppendLine(queryHintCount.ToString());

				if (queryHintCount > 0)
					foreach (var hint in queryHints)
						Builder.AppendLine(hint);

			    foreach (var elementType in QueryVisitor.FindOnce<IQueryElement>(query))
			    {
			        Visit(elementType);
			    }

				foreach (var parameter in parameters.Where(parameter => !Dic.ContainsKey(parameter)))
				{
				    Visit(parameter);
				}

				Builder
					.Append(++Index)
					.Append(' ')
					.Append(ParamIndex);

				Append(parameters.Length);

				foreach (var parameter in parameters)
					Append(parameter);

				Builder.AppendLine();

				return Builder.ToString();
			}

			void Visit(IQueryElement e)
			{
				switch (e.ElementType)
				{
					case EQueryElementType.SqlField :
						{
							var fld = (SqlField)e;

							if (fld != fld.Table.All)
								GetType(fld.SystemType);

							break;
						}

					case EQueryElementType.SqlParameter :
						{
							var p = (SqlParameter)e;
							var v = p.Value;
							var t = v == null ? p.SystemType : v.GetType();

							if (v == null || t.IsArray || t == typeof(string) || !(v is IEnumerable))
							{
								GetType(t);
							}
							else
							{
								var elemType = t.GetItemType();
								GetType(GetArrayType(elemType));
							}

							break;
						}

					case EQueryElementType.SqlFunction         : GetType(((SqlFunction)        e).SystemType); break;
					case EQueryElementType.SqlExpression       : GetType(((SqlExpression)      e).SystemType); break;
					case EQueryElementType.SqlBinaryExpression : GetType(((SqlBinaryExpression)e).SystemType); break;
					case EQueryElementType.SqlDataType         : GetType(((SqlDataType)        e).Type);       break;
					case EQueryElementType.SqlValue            : GetType(((SqlValue)           e).SystemType); break;
					case EQueryElementType.SqlTable            : GetType(((SqlTable)           e).ObjectType); break;
				}

				Dic.Add(e, ++Index);

				Builder
					.Append(Index)
					.Append(' ')
					.Append((int)e.ElementType);

				switch (e.ElementType)
				{
					case EQueryElementType.SqlField :
						{
							var elem = (SqlField)e;

							Append(elem.SystemType);
							Append(elem.Name);
							Append(elem.PhysicalName);
							Append(elem.Nullable);
							Append(elem.IsPrimaryKey);
							Append(elem.PrimaryKeyOrder);
							Append(elem.IsIdentity);
							Append(elem.IsUpdatable);
							Append(elem.IsInsertable);
							Append((int)elem.DataType);
							Append(elem.DbType);
							Append(elem.Length);
							Append(elem.Precision);
							Append(elem.Scale);
							Append(elem.CreateFormat);

							break;
						}

					case EQueryElementType.SqlFunction :
						{
							var elem = (SqlFunction)e;

							Append(elem.SystemType);
							Append(elem.Name);
							Append(elem.Precedence);
							Append(elem.Parameters);

							break;
						}

					case EQueryElementType.SqlParameter :
						{
							var elem = (SqlParameter)e;

							Append(elem.Name);
							Append(elem.IsQueryParameter);
							Append((int)elem.DataType);
							Append(elem.DbSize);
							Append(elem.LikeStart);
							Append(elem.LikeEnd);
							Append(elem.ReplaceLike);

							var value = elem.LikeStart != null ? elem.RawValue : elem.Value;
							var type  = value == null ? elem.SystemType : value.GetType();

							if (value == null || type.IsArray || type == typeof(string) || !(value is IEnumerable))
							{
								Append(type, value);
							}
							else
							{
								var elemType = type.GetItemType();

								value = ConvertIEnumerableToArray(value, elemType);

								Append(GetArrayType(elemType), value);
							}

							break;
						}

					case EQueryElementType.SqlExpression :
						{
							var elem = (SqlExpression)e;

							Append(elem.SystemType);
							Append(elem.Expr);
							Append(elem.Precedence);
							Append(elem.Parameters);

							break;
						}

					case EQueryElementType.SqlBinaryExpression :
						{
							var elem = (SqlBinaryExpression)e;

							Append(elem.SystemType);
							Append(elem.Expr1);
							Append(elem.Operation);
							Append(elem.Expr2);
							Append(elem.Precedence);

							break;
						}

					case EQueryElementType.SqlValue :
						{
							var elem = (SqlValue)e;
							Append(elem.SystemType, elem.Value);
							break;
						}

					case EQueryElementType.SqlDataType :
						{
							var elem = (SqlDataType)e;

							Append((int)elem.DataType);
							Append(elem.Type);
							Append(elem.Length);
							Append(elem.Precision);
							Append(elem.Scale);

							break;
						}

					case EQueryElementType.SqlTable :
						{
							var elem = (SqlTable)e;

							Append(elem.SourceID);
							Append(elem.Name);
							Append(elem.Alias);
							Append(elem.Database);
							Append(elem.Owner);
							Append(elem.PhysicalName);
							Append(elem.ObjectType);

							if (elem.SequenceAttributes.IsNullOrEmpty())
								Builder.Append(" -");
							else
							{
								Append(elem.SequenceAttributes.Length);

								foreach (var a in elem.SequenceAttributes)
								{
									Append(a.Configuration);
									Append(a.SequenceName);
								}
							}

							Append(Dic[elem.All]);
							Append(elem.Fields.Count);

							foreach (var field in elem.Fields)
								Append(Dic[field.Value]);

							Append((int)elem.SqlTableType);

							if (elem.SqlTableType != SqlTableType.Table)
							{
								if (elem.TableArguments == null)
									Append(0);
								else
								{
									Append(elem.TableArguments.Length);

									foreach (var expr in elem.TableArguments)
										Append(Dic[expr]);
								}
							}

							break;
						}

					case EQueryElementType.ExprPredicate :
						{
							var elem = (IExpr)e;

							Append(elem.Expr1);
							Append(elem.Precedence);

							break;
						}

					case EQueryElementType.NotExprPredicate :
						{
							var elem = (INotExpr)e;

							Append(elem.Expr1);
							Append(elem.IsNot);
							Append(elem.Precedence);

							break;
						}

					case EQueryElementType.ExprExprPredicate :
						{
							var elem = (IExprExpr)e;

							Append(elem.Expr1);
							Append((int)elem.EOperator);
							Append(elem.Expr2);

							break;
						}

					case EQueryElementType.LikePredicate :
						{
							var elem = (ILike)e;

							Append(elem.Expr1);
							Append(elem.IsNot);
							Append(elem.Expr2);
							Append(elem.Escape);

							break;
						}

					case EQueryElementType.BetweenPredicate :
						{
							var elem = (Between)e;

							Append(elem.Expr1);
							Append(elem.IsNot);
							Append(elem.Expr2);
							Append(elem.Expr3);

							break;
						}

					case EQueryElementType.IsNullPredicate :
						{
							var elem = (IIsNull)e;

							Append(elem.Expr1);
							Append(elem.IsNot);

							break;
						}

					case EQueryElementType.InSubQueryPredicate :
						{
							var elem = (IInSubQuery)e;

							Append(elem.Expr1);
							Append(elem.IsNot);
							Append(elem.SubQuery);

							break;
						}

					case EQueryElementType.InListPredicate :
						{
							var elem = (IInList)e;

							Append(elem.Expr1);
							Append(elem.IsNot);
							Append(elem.Values);

							break;
						}

					case EQueryElementType.FuncLikePredicate :
						{
							var elem = (IFuncLike)e;
							Append(elem.Function);
							break;
						}

					case EQueryElementType.SqlQuery :
						{
							var elem = (ISelectQuery)e;

							Append(elem.SourceID);
							Append((int)elem.EQueryType);
							Append(elem.From);

							var appendInsert      = false;
							var appendUpdate      = false;
							var appendDelete      = false;
							var appendSelect      = false;
							var appendCreateTable = false;

							switch (elem.EQueryType)
							{
								case EQueryType.InsertOrUpdate :
									appendUpdate = true;
									appendInsert = true;
									break;

								case EQueryType.Update         :
									appendUpdate = true;
									appendSelect = elem.Select.SkipValue != null || elem.Select.TakeValue != null;
									break;

								case EQueryType.Delete         :
									appendDelete = true;
									appendSelect = true;
									break;

								case EQueryType.Insert         :
									appendInsert = true;
									if (elem.From.Tables.Count != 0)
										appendSelect = true;
									break;

								case EQueryType.CreateTable    :
									appendCreateTable = true;
									break;

								default                       :
									appendSelect = true;
									break;
							}

							Append(appendInsert);      if (appendInsert)      Append(elem.Insert);
							Append(appendUpdate);      if (appendUpdate)      Append(elem.Update);
							Append(appendDelete);      if (appendDelete)      Append(elem.Delete);
							Append(appendSelect);      if (appendSelect)      Append(elem.Select);
							Append(appendCreateTable); if (appendCreateTable) Append(elem.CreateTable);

							Append(elem.Where);
							Append(elem.GroupBy);
							Append(elem.Having);
							Append(elem.OrderBy);
							Append(elem.ParentSelect == null ? 0 : elem.ParentSelect.SourceID);
							Append(elem.IsParameterDependent);

							if (!elem.HasUnion)
								Builder.Append(" -");
							else
								Append(elem.Unions);

							Append(elem.Parameters);

							if (Dic.ContainsKey(elem.All))
								Append(Dic[elem.All]);
							else
								Builder.Append(" -");

							break;
						}

					case EQueryElementType.Column :
						{
							var elem = (IColumn) e;

							Append(elem.Parent.SourceID);
							Append(elem.Expression);
							Append(elem.Alias);

							break;
						}

					case EQueryElementType.SearchCondition :
						{
							Append(((ISearchCondition)e).Conditions);
							break;
						}

					case EQueryElementType.Condition :
						{
							var elem = (Condition)e;

							Append(elem.IsNot);
							Append(elem.Predicate);
							Append(elem.IsOr);

							break;
						}

					case EQueryElementType.TableSource :
						{
							var elem = (ITableSource)e;

							Append(elem.Source);
							Append(elem.Alias);
							Append(elem.Joins);

							break;
						}

					case EQueryElementType.JoinedTable :
						{
							var elem = (IJoinedTable)e;

							Append((int)elem.JoinType);
							Append(elem.Table);
							Append(elem.IsWeak);
							Append(elem.Condition);

							break;
						}

					case EQueryElementType.SelectClause :
						{
							var elem = (ISelectClause)e;

							Append(elem.IsDistinct);
							Append(elem.SkipValue);
							Append(elem.TakeValue);
							Append(elem.Columns);

							break;
						}

					case EQueryElementType.InsertClause :
						{
							var elem = (IInsertClause)e;

							Append(elem.Items);
							Append(elem.Into);
							Append(elem.WithIdentity);

							break;
						}

					case EQueryElementType.UpdateClause :
						{
							var elem = (IUpdateClause)e;

							Append(elem.Items);
							Append(elem.Keys);
							Append(elem.Table);

							break;
						}

					case EQueryElementType.DeleteClause :
						{
							Append(((DeleteClause)e).Table);
							break;
						}

					case EQueryElementType.SetExpression :
						{
							var elem = (ISetExpression)e;

							Append(elem.Column);
							Append(elem.Expression);

							break;
						}

					case EQueryElementType.CreateTableStatement :
						{
							var elem = (ICreateTableStatement)e;

							Append(elem.Table);
							Append(elem.IsDrop);
							Append(elem.StatementHeader);
							Append(elem.StatementFooter);
							Append((int)elem.EDefaulNullable);

							break;
						}

					case EQueryElementType.FromClause    : Append(((IFromClause)   e).Tables);          break;
					case EQueryElementType.WhereClause   : Append(((IWhereClause)  e).SearchCondition); break;
					case EQueryElementType.GroupByClause : Append(((GroupByClause)e).Items);           break;
					case EQueryElementType.OrderByClause : Append(((IOrderByClause)e).Items);           break;

					case EQueryElementType.OrderByItem :
						{
							var elem = (IOrderByItem)e;

							Append(elem.Expression);
							Append(elem.IsDescending);

							break;
						}

					case EQueryElementType.Union :
						{
							var elem = (IUnion)e;

							Append(elem.SelectQuery);
							Append(elem.IsAll);

							break;
						}
				}

				Builder.AppendLine();
			}

			void Append<T>(ICollection<T> exprs)
				where T : IQueryElement
			{
				if (exprs == null)
					Builder.Append(" -");
				else
				{
					Append(exprs.Count);

					foreach (var e in exprs)
						Append(Dic[e]);
				}
			}
		}

		#endregion

		#region QueryDeserializer

		public class QueryDeserializer : DeserializerBase
		{
            ISelectQuery _query;
			SqlParameter[] _parameters;

			readonly Dictionary<int, ISelectQuery> _queries = new Dictionary<int, ISelectQuery>();
			readonly List<Action>                _actions = new List<Action>();

			public LinqServiceQuery Deserialize(string str)
			{
				Str = str;

				List<string> queryHints = null;

				var queryHintCount = ReadInt();

				NextLine();

				if (queryHintCount > 0)
				{
					queryHints = new List<string>();

					for (var i = 0; i < queryHintCount; i++)
					{
						var pos = Pos;

						while (Pos < Str.Length && Peek() != '\n' && Peek() != '\r')
							Pos++;

						queryHints.Add(Str.Substring(pos, Pos - pos));

						NextLine();
					}
				}

				while (Parse()) {}

				foreach (var action in _actions)
					action();

				return new LinqServiceQuery { Query = _query, Parameters = _parameters, QueryHints = queryHints };
			}

			bool Parse()
			{
				NextLine();

				if (Pos >= Str.Length)
					return false;

				var obj  = null as object;
				var idx  = ReadInt(); Pos++;
				var type = ReadInt(); Pos++;

				switch ((EQueryElementType)type)
				{
					case (EQueryElementType)ParamIndex     : obj = _parameters = ReadArray<SqlParameter>(); break;
					case (EQueryElementType)TypeIndex      : obj = ResolveType(ReadString());               break;
					case (EQueryElementType)TypeArrayIndex : obj = GetArrayType(Read<Type>());              break;

					case EQueryElementType.SqlField :
						{
							var systemType       = Read<Type>();
							var name             = ReadString();
							var physicalName     = ReadString();
							var nullable         = ReadBool();
							var isPrimaryKey     = ReadBool();
							var primaryKeyOrder  = ReadInt();
							var isIdentity       = ReadBool();
							var isUpdatable      = ReadBool();
							var isInsertable     = ReadBool();
							var dataType         = ReadInt();
							var dbType           = ReadString();
							var length           = ReadNullableInt();
							var precision        = ReadNullableInt();
							var scale            = ReadNullableInt();
							var createFormat     = ReadString();

							obj = new SqlField
							{
								SystemType      = systemType,
								Name            = name,
								PhysicalName    = physicalName,
								Nullable        = nullable,
								IsPrimaryKey    = isPrimaryKey,
								PrimaryKeyOrder = primaryKeyOrder,
								IsIdentity      = isIdentity,
								IsInsertable    = isInsertable,
								IsUpdatable     = isUpdatable,
								DataType        = (DataType)dataType,
								DbType          = dbType,
								Length          = length,
								Precision       = precision,
								Scale           = scale,
								CreateFormat    = createFormat,
							};

							break;
						}

					case EQueryElementType.SqlFunction :
						{
							var systemType = Read<Type>();
							var name       = ReadString();
							var precedence = ReadInt();
							var parameters = ReadArray<ISqlExpression>();

							obj = new SqlFunction(systemType, name, precedence, parameters);

							break;
						}

					case EQueryElementType.SqlParameter :
						{
							var name             = ReadString();
							var isQueryParameter = ReadBool();
							var dbType           = (DataType)ReadInt();
							var dbSize           = ReadInt();
							var likeStart        = ReadString();
							var likeEnd          = ReadString();
							var replaceLike      = ReadBool();

							var systemType       = Read<Type>();
							var value            = ReadValue(systemType);

							obj = new SqlParameter(systemType, name, value)
							{
								IsQueryParameter = isQueryParameter,
								DataType         = dbType,
								DbSize           = dbSize,
								LikeStart        = likeStart,
								LikeEnd          = likeEnd,
								ReplaceLike      = replaceLike,
							};

							break;
						}

					case EQueryElementType.SqlExpression :
						{
							var systemType = Read<Type>();
							var expr       = ReadString();
							var precedence = ReadInt();
							var parameters = ReadArray<ISqlExpression>();

							obj = new SqlExpression(systemType, expr, precedence, parameters);

							break;
						}

					case EQueryElementType.SqlBinaryExpression :
						{
							var systemType = Read<Type>();
							var expr1      = Read<ISqlExpression>();
							var operation  = ReadString();
							var expr2      = Read<ISqlExpression>();
							var precedence = ReadInt();

							obj = new SqlBinaryExpression(systemType, expr1, operation, expr2, precedence);

							break;
						}

					case EQueryElementType.SqlValue :
						{
							var systemType = Read<Type>();
							var value      = ReadValue(systemType);

							obj = new SqlValue(systemType, value);

							break;
						}

					case EQueryElementType.SqlDataType :
						{
							var dbType     = (DataType)ReadInt();
							var systemType = Read<Type>();
							var length     = ReadNullableInt();
							var precision  = ReadNullableInt();
							var scale      = ReadNullableInt();

							obj = new SqlDataType(dbType, systemType, length, precision, scale);

							break;
						}

					case EQueryElementType.SqlTable :
						{
							var sourceID           = ReadInt();
							var name               = ReadString();
							var alias              = ReadString();
							var database           = ReadString();
							var owner              = ReadString();
							var physicalName       = ReadString();
							var objectType         = Read<Type>();
							var sequenceAttributes = null as SequenceNameAttribute[];

							var count = ReadCount();

							if (count != null)
							{
								sequenceAttributes = new SequenceNameAttribute[count.Value];

								for (var i = 0; i < count.Value; i++)
									sequenceAttributes[i] = new SequenceNameAttribute(ReadString(), ReadString());
							}

							var all    = Read<SqlField>();
							var fields = ReadArray<SqlField>();
							var flds   = new SqlField[fields.Length + 1];

							flds[0] = all;
							Array.Copy(fields, 0, flds, 1, fields.Length);

							var sqlTableType = (SqlTableType)ReadInt();
							var tableArgs    = sqlTableType == SqlTableType.Table ? null : ReadArray<ISqlExpression>();

							obj = new SqlTable(
								sourceID, name, alias, database, owner, physicalName, objectType, sequenceAttributes, flds,
								sqlTableType, tableArgs);

							break;
						}

					case EQueryElementType.ExprPredicate :
						{
							var expr1      = Read<ISqlExpression>();
							var precedence = ReadInt();

							obj = new Expr(expr1, precedence);

							break;
						}

					case EQueryElementType.NotExprPredicate :
						{
							var expr1      = Read<ISqlExpression>();
							var isNot      = ReadBool();
							var precedence = ReadInt();

							obj = new NotExpr(expr1, isNot, precedence);

							break;
						}

					case EQueryElementType.ExprExprPredicate :
						{
							var expr1     = Read<ISqlExpression>();
							var @operator = (EOperator)ReadInt();
							var expr2     = Read<ISqlExpression>();

							obj = new ExprExpr(expr1, @operator, expr2);

							break;
						}

					case EQueryElementType.LikePredicate :
						{
							var expr1  = Read<ISqlExpression>();
							var isNot  = ReadBool();
							var expr2  = Read<ISqlExpression>();
							var escape = Read<ISqlExpression>();

							obj = new Like(expr1, isNot, expr2, escape);

							break;
						}

					case EQueryElementType.BetweenPredicate :
						{
							var expr1 = Read<ISqlExpression>();
							var isNot = ReadBool();
							var expr2 = Read<ISqlExpression>();
							var expr3 = Read<ISqlExpression>();

							obj = new Between(expr1, isNot, expr2, expr3);

							break;
						}

					case EQueryElementType.IsNullPredicate :
						{
							var expr1 = Read<ISqlExpression>();
							var isNot = ReadBool();

							obj = new IsNull(expr1, isNot);

							break;
						}

					case EQueryElementType.InSubQueryPredicate :
						{
							var expr1    = Read<ISqlExpression>();
							var isNot    = ReadBool();
							var subQuery = Read<ISelectQuery>();

							obj = new InSubQuery(expr1, isNot, subQuery);

							break;
						}

					case EQueryElementType.InListPredicate :
						{
							var expr1  = Read<ISqlExpression>();
							var isNot  = ReadBool();
							var values = ReadList<ISqlExpression>();

							obj = new InList(expr1, isNot, values);

							break;
						}

					case EQueryElementType.FuncLikePredicate :
						{
							var func = Read<SqlFunction>();
							obj = new FuncLike(func);
							break;
						}

					case EQueryElementType.SqlQuery :
						{
							var sid                = ReadInt();
							var queryType          = (EQueryType)ReadInt();
							var from               = Read<IFromClause>();
							var readInsert         = ReadBool();
							var insert             = readInsert ? Read<IInsertClause>() : null;
							var readUpdate         = ReadBool();
							var update             = readUpdate ? Read<UpdateClause>() : null;
							var readDelete         = ReadBool();
							var delete             = readDelete ? Read<DeleteClause>() : null;
							var readSelect         = ReadBool();
							var select             = readSelect ? Read<ISelectClause>() : new SelectClause(null);
							var readCreateTable    = ReadBool();
							var createTable        = readCreateTable ?
								Read<ICreateTableStatement>() :
								new CreateTableStatement();
							var where              = Read<IWhereClause>();
							var groupBy            = Read<GroupByClause>();
							var having             = Read<IWhereClause>();
							var orderBy            = Read<IOrderByClause>();
							var parentSql          = ReadInt();
							var parameterDependent = ReadBool();
							var unions             = ReadArray<IUnion>();
							var parameters         = ReadArray<SqlParameter>();

							var query = _query = new SelectQuery(sid) { EQueryType = queryType };

							query.Init(
								insert,
								update,
								delete,
								select,
								from,
								where,
								groupBy,
								having,
								orderBy,
								unions == null ? null : unions.ToList(),
								null,
								createTable,
								parameterDependent,
								parameters.ToList());

							_queries.Add(sid, _query);

							if (parentSql != 0)
								_actions.Add(() =>
								{
                                    ISelectQuery selectQuery;
									if (_queries.TryGetValue(parentSql, out selectQuery))
										query.ParentSelect = selectQuery;
								});

							query.All = Read<SqlField>();

							obj = query;

							break;
						}

					case EQueryElementType.Column :
						{
							var sid        = ReadInt();
							var expression = Read<ISqlExpression>();
							var alias      = ReadString();

							var col = new Column(null, expression, alias);

							_actions.Add(() => { col.Parent = _queries[sid]; return; });

							obj = col;

							break;
						}

					case EQueryElementType.SearchCondition :
						obj = new SearchCondition(ReadArray<Condition>());
						break;

					case EQueryElementType.Condition :
						obj = new Condition(ReadBool(), Read<ISqlPredicate>(), ReadBool());
						break;

					case EQueryElementType.TableSource :
						{
							var source = Read<ISqlTableSource>();
							var alias  = ReadString();
							var joins  = ReadArray<IJoinedTable>();

							obj = new TableSource(source, alias, joins);

							break;
						}

					case EQueryElementType.JoinedTable :
						{
							var joinType  = (EJoinType)ReadInt();
							var table     = Read<ITableSource>();
							var isWeak    = ReadBool();
							var condition = Read<ISearchCondition>();

							obj = new JoinedTable(joinType, table, isWeak, condition);

							break;
						}

					case EQueryElementType.SelectClause :
						{
							var isDistinct = ReadBool();
							var skipValue  = Read<ISqlExpression>();
							var takeValue  = Read<ISqlExpression>();
							var columns    = ReadArray<IColumn>();

							obj = new SelectClause(isDistinct, takeValue, skipValue, columns);

							break;
						}

					case EQueryElementType.InsertClause :
						{
							var items = ReadArray<ISetExpression>();
							var into  = Read<SqlTable>();
							var wid   = ReadBool();

							var c = new InsertClause { Into = into, WithIdentity = wid };

							c.Items.AddRange(items);
							obj = c;

							break;
						}

					case EQueryElementType.UpdateClause :
						{
							var items = ReadArray<ISetExpression>();
							var keys  = ReadArray<ISetExpression>();
							var table = Read<SqlTable>();

							var c = new UpdateClause { Table = table };

							c.Items.AddRange(items);
							c.Keys. AddRange(keys);
							obj = c;

							break;
						}

					case EQueryElementType.DeleteClause :
						{
							var table = Read<SqlTable>();
							obj = new DeleteClause { Table = table };
							break;
						}

					case EQueryElementType.CreateTableStatement :
						{
							var table           = Read<SqlTable>();
							var isDrop          = ReadBool();
							var statementHeader = ReadString();
							var statementFooter = ReadString();
							var defaultNullable = (EDefaulNullable)ReadInt();

							obj = new CreateTableStatement
							{
								Table           = table,
								IsDrop          = isDrop,
								StatementHeader = statementHeader,
								StatementFooter = statementFooter,
								EDefaulNullable  = defaultNullable,
							};

							break;
						}

					case EQueryElementType.SetExpression : obj = new SetExpression(Read     <ISqlExpression>(), Read<ISqlExpression>()); break;
					case EQueryElementType.FromClause    : obj = new FromClause   (ReadArray<ITableSource>());                break;
					case EQueryElementType.WhereClause   : obj = new WhereClause  (Read     <ISearchCondition>());            break;
					case EQueryElementType.GroupByClause : obj = new GroupByClause(ReadArray<ISqlExpression>());                         break;
					case EQueryElementType.OrderByClause : obj = new OrderByClause(ReadArray<IOrderByItem>());                break;

					case EQueryElementType.OrderByItem :
						{
							var expression   = Read<ISqlExpression>();
							var isDescending = ReadBool();

							obj = new OrderByItem(expression, isDescending);

							break;
						}

					case EQueryElementType.Union :
						{
							var sqlQuery = Read<ISelectQuery>();
							var isAll    = ReadBool();

							obj = new Union(sqlQuery, isAll);

							break;
						}
				}

				Dic.Add(idx, obj);

				return true;
			}
		}

		#endregion

		#region ResultSerializer

		class ResultSerializer : SerializerBase
		{
			public string Serialize(LinqServiceResult result)
			{
				Append(result.FieldCount);
				Append(result.VaryingTypes.Length);
				Append(result.RowCount);
				Append(result.QueryID.ToString());

				Builder.AppendLine();

				foreach (var name in result.FieldNames)
				{
					Append(name);
					Builder.AppendLine();
				}

				foreach (var type in result.FieldTypes)
				{
					Append(type.FullName);
					Builder.AppendLine();
				}

				foreach (var type in result.VaryingTypes)
				{
					Append(type.FullName);
					Builder.AppendLine();
				}

				foreach (var data in result.Data)
				{
					foreach (var str in data)
					{
						if (result.VaryingTypes.Length > 0 && !string.IsNullOrEmpty(str) && str[0] == '\0')
						{
							Builder.Append('*');
							Append(str[1]);
							Append(str.Substring(2));
						}
						else
							Append(str);
					}

					Builder.AppendLine();
				}

				return Builder.ToString();
			}
		}

		#endregion

		#region ResultDeserializer

		class ResultDeserializer : DeserializerBase
		{
			public LinqServiceResult DeserializeResult(string str)
			{
				Str = str;

				var fieldCount  = ReadInt();
				var varTypesLen = ReadInt();

				var result = new LinqServiceResult
				{
					FieldCount   = fieldCount,
					RowCount     = ReadInt(),
					VaryingTypes = new Type[varTypesLen],
					QueryID      = new Guid(ReadString()),
					FieldNames   = new string[fieldCount],
					FieldTypes   = new Type  [fieldCount],
					Data         = new List<string[]>(),
				};

				NextLine();

				for (var i = 0; i < fieldCount;  i++) { result.FieldNames  [i] = ReadString();              NextLine(); }
				for (var i = 0; i < fieldCount;  i++) { result.FieldTypes  [i] = ResolveType(ReadString()); NextLine(); }
				for (var i = 0; i < varTypesLen; i++) { result.VaryingTypes[i] = ResolveType(ReadString()); NextLine(); }

				for (var n = 0; n < result.RowCount; n++)
				{
					var data = new string[fieldCount];

					for (var i = 0; i < fieldCount; i++)
					{
						if (varTypesLen > 0)
						{
							Get(' ');

							if (Get('*'))
							{
								var idx = ReadInt();
								data[i] = "\0" + (char)idx + ReadString();
							}
							else
								data[i] = ReadString();
						}
						else
							data[i] = ReadString();
					}

					result.Data.Add(data);

					NextLine();
				}

				return result;
			}
		}

		#endregion

		#region StringArraySerializer

		class StringArraySerializer : SerializerBase
		{
			public string Serialize(string[] data)
			{
				Append(data.Length);

				foreach (var str in data)
					Append(str);

				Builder.AppendLine();

				return Builder.ToString();
			}
		}

		#endregion

		#region StringArrayDeserializer

		class StringArrayDeserializer : DeserializerBase
		{
			public string[] Deserialize(string str)
			{
				Str = str;

				var data = new string[ReadInt()];

				for (var i = 0; i < data.Length; i++)
					data[i] = ReadString();

				return data;
			}
		}

		#endregion

		#region Helpers

		interface IArrayHelper
		{
			Type   GetArrayType();
			object ConvertToArray(object list);
		}

		class ArrayHelper<T> : IArrayHelper
		{
			public Type GetArrayType()
			{
				return typeof(T[]);
			}

			public object ConvertToArray(object list)
			{
				return ((IEnumerable<T>)list).ToArray();
			}
		}

		static readonly Dictionary<Type,Type>                _arrayTypes      = new Dictionary<Type,Type>();
		static readonly Dictionary<Type,Func<object,object>> _arrayConverters = new Dictionary<Type,Func<object,object>>();

		static Type GetArrayType(Type elementType)
		{
			Type arrayType;

			lock (_arrayTypes)
			{
				if (!_arrayTypes.TryGetValue(elementType, out arrayType))
				{
					var helper = (IArrayHelper)Activator.CreateInstance(typeof(ArrayHelper<>).MakeGenericType(elementType));
					_arrayTypes.Add(elementType, arrayType = helper.GetArrayType());
				}
			}

			return arrayType;
		}

		static object ConvertIEnumerableToArray(object list, Type elementType)
		{
			Func<object,object> converter;

			lock (_arrayConverters)
			{
				if (!_arrayConverters.TryGetValue(elementType, out converter))
				{
					var helper = (IArrayHelper)Activator.CreateInstance(typeof(ArrayHelper<>).MakeGenericType(elementType));
					_arrayConverters.Add(elementType, converter = helper.ConvertToArray);
				}
			}

			return converter(list);
		}

		#endregion
	}
}
