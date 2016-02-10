﻿using System.Data;
using System.Linq;
using System.Text;

namespace LinqToDB.DataProvider.DB2
{
    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    using SqlProvider;

	abstract class DB2SqlBuilderBase : BasicSqlBuilder
	{
		protected DB2SqlBuilderBase(ISqlOptimizer sqlOptimizer, SqlProviderFlags sqlProviderFlags, ValueToSqlConverter valueToSqlConverter)
			: base(sqlOptimizer, sqlProviderFlags, valueToSqlConverter)
		{
		}

        ISqlField _identityField;

		protected abstract DB2Version Version { get; }

		public override int CommandCount(ISelectQuery selectQuery)
		{
			if (Version == DB2Version.LUW && selectQuery.IsInsert && selectQuery.Insert.WithIdentity)
			{
				_identityField = selectQuery.Insert.Into.GetIdentityField();

				if (_identityField == null)
					return 2;
			}

			return 1;
		}

		protected override void BuildSql(int commandNumber, ISelectQuery selectQuery, StringBuilder sb, int indent, bool skipAlias)
		{
			SelectQuery   = selectQuery;
			StringBuilder = sb;
			Indent        = indent;
			SkipAlias     = skipAlias;

			if (_identityField != null)
			{
				indent += 2;

				AppendIndent().AppendLine("SELECT");
				AppendIndent().Append("\t");
				BuildExpression(_identityField, false, true);
				sb.AppendLine();
				AppendIndent().AppendLine("FROM");
				AppendIndent().AppendLine("\tNEW TABLE");
				AppendIndent().AppendLine("\t(");
			}

			base.BuildSql(commandNumber, selectQuery, sb, indent, skipAlias);

			if (_identityField != null)
				sb.AppendLine("\t)");
		}

		protected override void BuildGetIdentity()
		{
			if (Version == DB2Version.zOS)
			{
				StringBuilder
					.AppendLine(";")
					.AppendLine("SELECT IDENTITY_VAL_LOCAL() FROM SYSIBM.SYSDUMMY1");
			}
		}

		protected override void BuildCommand(int commandNumber)
		{
			StringBuilder.AppendLine("SELECT identity_val_local() FROM SYSIBM.SYSDUMMY1");
		}

		protected override void BuildSql()
		{
			AlternativeBuildSql(false, base.BuildSql);
		}

		protected override void BuildSelectClause()
		{
			if (SelectQuery.From.Tables.Count == 0)
			{
				AppendIndent().AppendLine("SELECT");
				BuildColumns();
				AppendIndent().AppendLine("FROM SYSIBM.SYSDUMMY1 FETCH FIRST 1 ROW ONLY");
			}
			else
				base.BuildSelectClause();
		}

		protected override string LimitFormat => SelectQuery.Select.SkipValue == null ? "FETCH FIRST {0} ROWS ONLY" : null;

	    protected override void BuildFunction(ISqlFunction func)
		{
			func = ConvertFunctionParameters(func);
			base.BuildFunction(func);
		}

		protected override void BuildFromClause()
		{
			if (!SelectQuery.IsUpdate)
				base.BuildFromClause();
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
					var ex = expr as ISqlExpression;
					wrap = ex != null && ex.Expr == "{0}" && ex.Parameters.Length == 1 && ex.Parameters[0] is ISearchCondition;
				}
			}

			if (wrap) StringBuilder.Append("CASE WHEN ");
			base.BuildColumnExpression(expr, alias, ref addAlias);
			if (wrap) StringBuilder.Append(" THEN 1 ELSE 0 END");
		}

		protected override void BuildDataType(ISqlDataType type, bool createDbType = false)
		{
			switch (type.DataType)
			{
				case DataType.DateTime  : StringBuilder.Append("timestamp"); break;
				case DataType.DateTime2 : StringBuilder.Append("timestamp"); break;
				default                 : base.BuildDataType(type);          break;
			}
		}

		public static DB2IdentifierQuoteMode IdentifierQuoteMode = DB2IdentifierQuoteMode.Auto;

		public override object Convert(object value, ConvertType convertType)
		{
			switch (convertType)
			{
				case ConvertType.NameToQueryParameter:
					return "@" + value;

				case ConvertType.NameToCommandParameter:
				case ConvertType.NameToSprocParameter:
					return ":" + value;

				case ConvertType.SprocParameterToName:
					if (value != null)
					{
						var str = value.ToString();
						return str.Length > 0 && str[0] == ':'? str.Substring(1): str;
					}

					break;

				case ConvertType.NameToQueryField:
				case ConvertType.NameToQueryFieldAlias:
				case ConvertType.NameToQueryTable:
				case ConvertType.NameToQueryTableAlias:
					if (value != null && IdentifierQuoteMode != DB2IdentifierQuoteMode.None)
					{
						var name = value.ToString();

						if (name.Length > 0 && name[0] == '"')
							return name;

						if (IdentifierQuoteMode == DB2IdentifierQuoteMode.Quote ||
							name.StartsWith("_") ||
							name
#if NETFX_CORE
								.ToCharArray()
#endif
								.Any(c => char.IsLower(c) || char.IsWhiteSpace(c)))
							return '"' + name + '"';
					}

					break;
			}

			return value;
		}

		protected override void BuildInsertOrUpdateQuery()
		{
			BuildInsertOrUpdateQueryAsMerge("FROM SYSIBM.SYSDUMMY1 FETCH FIRST 1 ROW ONLY");
		}

		protected override void BuildEmptyInsert()
		{
			StringBuilder.Append("VALUES ");

			foreach (var col in SelectQuery.Insert.Into.Fields)
				StringBuilder.Append("(DEFAULT)");

			StringBuilder.AppendLine();
		}

		protected override void BuildCreateTableIdentityAttribute1(ISqlField field)
		{
			StringBuilder.Append("GENERATED ALWAYS AS IDENTITY");
		}

#if !SILVERLIGHT

		protected override string GetProviderTypeName(IDbDataParameter parameter)
		{
			dynamic p = parameter;
			return p.DB2Type.ToString();
		}

#endif
	}
}
