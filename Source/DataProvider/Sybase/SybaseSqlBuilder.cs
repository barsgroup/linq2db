﻿using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LinqToDB.DataProvider.Sybase
{
    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    using SqlProvider;

	class SybaseSqlBuilder : BasicSqlBuilder
	{
		public SybaseSqlBuilder(ISqlOptimizer sqlOptimizer, SqlProviderFlags sqlProviderFlags, ValueToSqlConverter valueToSqlConverter)
			: base(sqlOptimizer, sqlProviderFlags, valueToSqlConverter)
		{
		}

		protected override void BuildGetIdentity()
		{
			StringBuilder
				.AppendLine()
				.AppendLine("SELECT @@IDENTITY");
		}

		protected override string FirstFormat => "TOP {0}";

	    protected override void BuildFunction(ISqlFunction func)
		{
			func = ConvertFunctionParameters(func);
			base.BuildFunction(func);
		}

		private  bool _isSelect;
		readonly bool _skipAliases;

		SybaseSqlBuilder(bool skipAliases, ISqlOptimizer sqlOptimizer, SqlProviderFlags sqlProviderFlags, ValueToSqlConverter valueToSqlConverter)
			: base(sqlOptimizer, sqlProviderFlags, valueToSqlConverter)
		{
			_skipAliases = skipAliases;
		}

		protected override void BuildSelectClause()
		{
			_isSelect = true;
			base.BuildSelectClause();
			_isSelect = false;
		}

		protected override void BuildColumnExpression(IQueryExpression expr, string alias, ref bool addAlias)
		{
			var wrap = false;

			if (expr.SystemType == typeof(bool))
			{
				if (expr is ISearchCondition)
					wrap = true;
				else
				{
					var ex = expr as SqlExpression;
					wrap = ex != null && ex.Expr == "{0}" && ex.Parameters.Length == 1 && ex.Parameters[0] is ISearchCondition;
				}
			}

			if (wrap) StringBuilder.Append("CASE WHEN ");
			base.BuildColumnExpression(expr, alias, ref addAlias);
			if (wrap) StringBuilder.Append(" THEN 1 ELSE 0 END");

			if (_skipAliases) addAlias = false;
		}

		protected override ISqlBuilder CreateSqlBuilder()
		{
			return new SybaseSqlBuilder(_isSelect, SqlOptimizer, SqlProviderFlags, ValueToSqlConverter);
		}

		protected override void BuildDataType(ISqlDataType type, bool createDbType = false)
		{
			switch (type.DataType)
			{
				case DataType.DateTime2 : StringBuilder.Append("DateTime"); break;
				default                 : base.BuildDataType(type); break;
			}
		}

		protected override void BuildDeleteClause()
		{
			AppendIndent();
			StringBuilder.Append("DELETE");
			BuildSkipFirst();
			StringBuilder.Append(" FROM ");

			ISqlTableSource table;
			ISqlTableSource source;

			if (SelectQuery.Delete.Table != null)
				table = source = SelectQuery.Delete.Table;
			else
			{
				table  = SelectQuery.From.Tables[0];
				source = SelectQuery.From.Tables[0].Source;
			}

			var alias = GetTableAlias(table);
			BuildPhysicalTable(source, alias);
	
			StringBuilder.AppendLine();
		}

		protected override void BuildLikePredicate(ILike predicate)
		{
		    var sqlValue = predicate.Expr2 as ISqlValue;
		    if (sqlValue != null)
			{
				var value = sqlValue.Value;

				if (value != null)
				{
					var text  = value.ToString();
					var ntext = text.Replace("[", "[[]");

					if (text != ntext)
						predicate = new Like(predicate.Expr1, predicate.IsNot, new SqlValue(ntext), predicate.Escape);
				}
			}
			else
		    {
		        var expr2 = predicate.Expr2 as ISqlParameter;
		        if (expr2 != null)
		        {
		            expr2.ReplaceLike = true;
		        }
		    }

		    base.BuildLikePredicate(predicate);
		}

		protected override void BuildUpdateTableName()
		{
			if (SelectQuery.Update.Table != null && SelectQuery.Update.Table != SelectQuery.From.Tables[0].Source)
				BuildPhysicalTable(SelectQuery.Update.Table, null);
			else
				BuildTableName(SelectQuery.From.Tables[0], true, false);
		}

		public override object Convert(object value, ConvertType convertType)
		{
			switch (convertType)
			{
				case ConvertType.NameToQueryParameter:
				case ConvertType.NameToCommandParameter:
				case ConvertType.NameToSprocParameter:
					{
						var name = "@" + value;

						if (name.Length > 27)
							name = name.Substring(0, 27);

						return name;
					}

				case ConvertType.NameToQueryField:
				case ConvertType.NameToQueryFieldAlias:
				case ConvertType.NameToQueryTableAlias:
					{
						var name = value.ToString();

						if (name.Length > 28 || name.Length > 0 && name[0] == '[')
							return value;
					}

					return "[" + value + "]";

				case ConvertType.NameToDatabase:
				case ConvertType.NameToOwner:
				case ConvertType.NameToQueryTable:
					if (value != null)
					{
						var name = value.ToString();

						if (name.Length > 28 || name.Length > 0 && (name[0] == '[' || name[0] == '#'))
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

		protected override void BuildInsertOrUpdateQuery()
		{
			BuildInsertOrUpdateQueryAsUpdateInsert();
		}

		protected override void BuildEmptyInsert()
		{
			StringBuilder.AppendLine("VALUES ()");
		}

		protected override void BuildCreateTableIdentityAttribute1(ISqlField field)
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

#if !SILVERLIGHT

		protected override string GetProviderTypeName(IDbDataParameter parameter)
		{
			dynamic p = parameter;
			return p.AseDbType.ToString();
		}

#endif
	}
}
