using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace LinqToDB.DataProvider.Access
{
	using Extensions;

	using LinqToDB.SqlQuery.QueryElements;
	using LinqToDB.SqlQuery.QueryElements.Conditions;
	using LinqToDB.SqlQuery.QueryElements.Interfaces;
	using LinqToDB.SqlQuery.QueryElements.Predicates;
	using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
	using LinqToDB.SqlQuery.QueryElements.SqlElements;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

	using SqlQuery;
	using SqlProvider;

	class AccessSqlBuilder : BasicSqlBuilder
	{
		public AccessSqlBuilder(ISqlOptimizer sqlOptimizer, SqlProviderFlags sqlProviderFlags, ValueToSqlConverter valueToSqlConverter)
			: base(sqlOptimizer, sqlProviderFlags, valueToSqlConverter)
		{
		}

		public override int CommandCount(ISelectQuery selectQuery)
		{
			return selectQuery.IsInsert && selectQuery.Insert.WithIdentity ? 2 : 1;
		}

		protected override void BuildCommand(int commandNumber)
		{
			StringBuilder.AppendLine("SELECT @@IDENTITY");
		}

		public override bool IsNestedJoinSupported => false;

	    #region Skip / Take Support

		protected override string FirstFormat => "TOP {0}";

	    protected override void BuildSql()
		{
			if (NeedSkip)
			{
				AlternativeBuildSql2(base.BuildSql);
				return;
			}

			if (SelectQuery.From.Tables.Count == 0 && SelectQuery.Select.Columns.Count == 1)
			{
			    var sqlFunction = SelectQuery.Select.Columns[0].Expression as ISqlFunction;
			    if (sqlFunction != null)
				{
				    if (sqlFunction.Parameters.Length == 3)
                    {
                        var searchCondition = sqlFunction.Parameters[0] as ISearchCondition;
                        if (sqlFunction.Name == "Iif" && searchCondition != null)
                        {
                            if (searchCondition.Conditions.Count == 1)
                            {
                                var like = searchCondition.Conditions.First.Value.Predicate as IFuncLike;
                                if (like != null && like.Function.Name == "EXISTS")
                                {
                                    BuildAnyAsCount();
                                    return;
                                }
                            }

                        }
                    }
				}
				else
			    {
			        var searchCondition = SelectQuery.Select.Columns[0].Expression as ISearchCondition;

                    if (searchCondition?.Conditions.Count == 1)
                    {
                        var like = searchCondition.Conditions.First.Value.Predicate as IFuncLike;
                        if (like != null)
			            {
                            if (like.Function.Name == "EXISTS")
                            {
                                BuildAnyAsCount();
                                return;
                            }
                        }
                    }
			    }
			}

		    base.BuildSql();
		}

        IColumn _selectColumn;

		void BuildAnyAsCount()
		{
            ISearchCondition cond;

		    var sqlFunction = SelectQuery.Select.Columns[0].Expression as ISqlFunction;
		    if (sqlFunction != null)
			{
				cond  = (ISearchCondition)sqlFunction.Parameters[0];
			}
			else
			{
				cond  = (ISearchCondition)SelectQuery.Select.Columns[0].Expression;
			}

			var exist = ((IFuncLike)cond.Conditions.First.Value.Predicate).Function;
			var query = (ISelectQuery)exist.Parameters[0];

			_selectColumn = new Column(SelectQuery, new SqlExpression(cond.Conditions.First.Value.IsNot ? "Count(*) = 0" : "Count(*) > 0"), SelectQuery.Select.Columns[0].Alias);

			BuildSql(0, query, StringBuilder);

			_selectColumn = null;
		}

		protected override IEnumerable<IColumn> GetSelectedColumns()
		{
			if (_selectColumn != null)
				return new[] { _selectColumn };

			if (NeedSkip && !SelectQuery.OrderBy.IsEmpty)
				return AlternativeGetSelectedColumns(base.GetSelectedColumns);

			return base.GetSelectedColumns();
		}

		protected override void BuildSkipFirst()
		{
			if (NeedSkip)
			{
				if (!NeedTake)
				{
					StringBuilder.AppendFormat(" TOP {0}", int.MaxValue);
				}
				else if (!SelectQuery.OrderBy.IsEmpty)
				{
					StringBuilder.Append(" TOP ");
					BuildExpression(Add<int>(SelectQuery.Select.SkipValue, SelectQuery.Select.TakeValue));
				}
			}
			else
				base.BuildSkipFirst();
		}

		#endregion

		protected override ISqlBuilder CreateSqlBuilder()
		{
			return new AccessSqlBuilder(SqlOptimizer, SqlProviderFlags, ValueToSqlConverter);
		}

		protected override bool ParenthesizeJoin()
		{
			return true;
		}

		protected override void BuildLikePredicate(ILike predicate)
		{
		    var sqlValue = predicate.Expr2 as ISqlValue;
		    if (sqlValue != null)
			{
				var value = sqlValue.Value;

				if (value != null)
				{
					var text  = sqlValue.Value.ToString();
					var ntext = text.Replace("[", "[[]");

					if (text != ntext)
						predicate = new Like(predicate.Expr1, predicate.IsNot, new SqlValue(ntext), predicate.Escape);
				}
			}
			else
		    {
		        var sqlParameter = predicate.Expr2 as ISqlParameter;
		        if (sqlParameter != null)
		        {
		            sqlParameter.ReplaceLike = true;
		        }
		    }

		    if (predicate.Escape != null)
		    {
		        var escape = predicate.Escape as ISqlValue;
		        if (escape != null)
				{
					var value = ((ISqlValue)predicate.Expr2).Value;

					if (value != null)
					{
						var text = ((ISqlValue)predicate.Expr2).Value.ToString();
						var val  = new SqlValue(ReescapeLikeText(text, (char)escape.Value));

						predicate = new Like(predicate.Expr1, predicate.IsNot, val, null);
					}
				}
				else
		        {
		            var p = predicate.Expr2 as ISqlParameter;

		            if (p?.LikeStart != null)
		            {
		                var value = (string)p.Value;

		                if (value != null)
		                {
		                    value     = value.Replace("[", "[[]").Replace("~%", "[%]").Replace("~_", "[_]").Replace("~~", "[~]");
		                    p         = new SqlParameter(p.SystemType, p.Name, value) { DbSize = p.DbSize, DataType = p.DataType, IsQueryParameter = p.IsQueryParameter };
		                    predicate = new Like(predicate.Expr1, predicate.IsNot, p, null);
		                }
		            }
		        }
		    }

		    base.BuildLikePredicate(predicate);
		}

		static string ReescapeLikeText(string text, char esc)
		{
			var sb = new StringBuilder(text.Length);

			for (var i = 0; i < text.Length; i++)
			{
				var c = text[i];

				if (c == esc)
				{
					sb.Append('[');
					sb.Append(text[++i]);
					sb.Append(']');
				}
				else if (c == '[')
					sb.Append("[[]");
				else
					sb.Append(c);
			}

			return sb.ToString();
		}

		protected override void BuildBinaryExpression(ISqlBinaryExpression expr)
		{
			switch (expr.Operation[0])
			{
				case '%': expr = new SqlBinaryExpression(expr.SystemType, expr.Expr1, "MOD", expr.Expr2, Precedence.Additive - 1); break;
				case '&':
				case '|':
				case '^': throw new SqlException("Operator '{0}' is not supported by the {1}.", expr.Operation, GetType().Name);
			}

			base.BuildBinaryExpression(expr);
		}

		protected override void BuildFunction(ISqlFunction func)
		{
			switch (func.Name)
			{
				case "Coalesce"  :

					if (func.Parameters.Length > 2)
					{
						var parms = new IQueryExpression[func.Parameters.Length - 1];

						Array.Copy(func.Parameters, 1, parms, 0, parms.Length);
						BuildFunction(new SqlFunction(func.SystemType, func.Name, func.Parameters[0],
						              new SqlFunction(func.SystemType, func.Name, parms)));
						return;
					}

					var sc = new SearchCondition();

					sc.Conditions.AddLast(new Condition(false, new IsNull(func.Parameters[0], false)));

					func = new SqlFunction(func.SystemType, "Iif", sc, func.Parameters[1], func.Parameters[0]);

					break;

				case "CASE"      : func = ConvertCase(func.SystemType, func.Parameters, 0); break;
				case "CharIndex" :
					func = func.Parameters.Length == 2?
						new SqlFunction(func.SystemType, "InStr", new SqlValue(1),    func.Parameters[1], func.Parameters[0], new SqlValue(1)):
						new SqlFunction(func.SystemType, "InStr", func.Parameters[2], func.Parameters[1], func.Parameters[0], new SqlValue(1));
					break;

				case "Convert"   :
					switch (func.SystemType.ToUnderlying().GetTypeCodeEx())
					{
						case TypeCode.String   : func = new SqlFunction(func.SystemType, "CStr",  func.Parameters[1]); break;
						case TypeCode.DateTime :
							if (IsDateDataType(func.Parameters[0], "Date"))
								func = new SqlFunction(func.SystemType, "DateValue", func.Parameters[1]);
							else if (IsTimeDataType(func.Parameters[0]))
								func = new SqlFunction(func.SystemType, "TimeValue", func.Parameters[1]);
							else
								func = new SqlFunction(func.SystemType, "CDate", func.Parameters[1]);
							break;

						default:
							if (func.SystemType == typeof(DateTime))
								goto case TypeCode.DateTime;

							BuildExpression(func.Parameters[1]);

							return;
					}

					break;
			}

			base.BuildFunction(func);
		}

		ISqlFunction ConvertCase(Type systemType, IQueryExpression[] parameters, int start)
		{
			var len = parameters.Length - start;

			if (len < 3)
				throw new SqlException("CASE statement is not supported by the {0}.", GetType().Name);

			if (len == 3)
				return new SqlFunction(systemType, "Iif", parameters[start], parameters[start + 1], parameters[start + 2]);

			return new SqlFunction(systemType, "Iif", parameters[start], parameters[start + 1], ConvertCase(systemType, parameters, start + 2));
		}

		protected override void BuildUpdateClause()
		{
			base.BuildFromClause();
			StringBuilder.Remove(0, 4).Insert(0, "UPDATE");
			base.BuildUpdateSet();
		}

		protected override void BuildFromClause()
		{
			if (!SelectQuery.IsUpdate)
				base.BuildFromClause();
		}

		protected override void BuildDataType(ISqlDataType type, bool createDbType = false)
		{
			switch (type.DataType)
			{
				case DataType.DateTime2 : StringBuilder.Append("timestamp"); break;
				default                 : base.BuildDataType(type);          break;
			}
		}

		public override object Convert(object value, ConvertType convertType)
		{
			switch (convertType)
			{
				case ConvertType.NameToQueryParameter:
				case ConvertType.NameToCommandParameter:
				case ConvertType.NameToSprocParameter:
					return "@" + value;

				case ConvertType.NameToQueryField:
				case ConvertType.NameToQueryFieldAlias:
				case ConvertType.NameToQueryTableAlias:
					{
						var name = value.ToString();

						if (name.Length > 0 && name[0] == '[')
							return value;
					}

					return "[" + value + "]";

				case ConvertType.NameToDatabase:
				case ConvertType.NameToOwner:
				case ConvertType.NameToQueryTable:
					if (value != null)
					{
						var name = value.ToString();

						if (name.Length > 0 && name[0] == '[')
							return value;

						if (name.IndexOf('.') > 0)
							value = string.Join("].[", name.Split('.'));

						return "[" + value + "]";
					}

					break;

				case ConvertType.SprocParameterToName:
					if (value != null)
					{
						var str = value.ToString();
						return str.Length > 0 && str[0] == '@'? str.Substring(1): str;
					}

					break;
			}

			return value;
		}

		protected override void BuildCreateTableIdentityAttribute2(ISqlField field)
		{
			StringBuilder.Append("IDENTITY");
		}

		protected override void BuildCreateTablePrimaryKey(string pkName, IEnumerable<string> fieldNames)
		{
			AppendIndent();
			StringBuilder.Append("CONSTRAINT ").Append(pkName).Append(" PRIMARY KEY CLUSTERED (");
			StringBuilder.Append(fieldNames.Aggregate((f1,f2) => f1 + ", " + f2));
			StringBuilder.Append(")");
		}

#if !NETFX_CORE && !SILVERLIGHT

		protected override string GetProviderTypeName(IDbDataParameter parameter)
		{
			return ((System.Data.OleDb.OleDbParameter)parameter).OleDbType.ToString();
		}

#endif
	}
}
