using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB.Mapping;

namespace LinqToDB.DataProvider.SapHana
{
	using System.Globalization;
	using System.Linq.Expressions;
	using System.Reflection;

	using LinqToDB.SqlEntities;
	using LinqToDB.SqlQuery.QueryElements.SqlElements;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Enums;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class CalculationViewInputParametersExpressionAttribute : Sql.TableExpressionAttribute
	{
		public CalculationViewInputParametersExpressionAttribute() :
			base("")
		{
		}

		// we can't use BasicSqlBuilder.GetValueBuilder, because
		// a) we need to escape with ' every value, 
		// b) we don't have dataprovider here ether
		private static string ValueToString(object value)
		{
		    var valueToString = value as string;
		    if (valueToString != null)
				return valueToString;
			if (value is decimal)
				return ((decimal)value).ToString(new NumberFormatInfo());
			if (value is double)
				return ((double)value).ToString(new NumberFormatInfo());
			if (value is float)
				return ((float)value).ToString(new NumberFormatInfo());
			return value.ToString();
		}

		public override void SetTable(MappingSchema mappingSchema, ISqlTable table, MemberInfo member, IEnumerable<Expression> expArgs, IEnumerable<IQueryExpression> sqlArgs)
		{
			var method = member as MethodInfo;

			if (method == null)
				throw new ArgumentNullException(nameof(member));

			var paramsList = method.GetParameters().ToList();
			var valuesList = expArgs.Cast<ConstantExpression>().ToList();

			if (paramsList.Count != valuesList.Count)
				throw new TargetParameterCountException("Invalid number of parameters");

			var sqlValues = new List<IQueryExpression>();

			for(var i = 0; i < paramsList.Count; i++)
			{
				var val = valuesList[i].Value;
				if (val == null)
					continue;
				var p = paramsList[i];
				sqlValues.Add(new SqlValue("$$" + p.Name + "$$"));
				sqlValues.Add(new SqlValue(ValueToString(val)));
			}

			var arg = new IQueryExpression[1];

			arg[0] = new SqlExpression(
				string.Join(", ",
					Enumerable.Range(0, sqlValues.Count)
						.Select(x => "{" + x + "}")),
				sqlValues.ToArray());

			table.SqlTableType = ESqlTableType.Expression;
			table.Name = "{0}('PLACEHOLDER' = {2}) {1}";
			table.TableArguments = arg.ToArray();
		}
	}
}
