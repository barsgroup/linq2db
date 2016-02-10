using System;

namespace LinqToDB.DataProvider.SqlCe
{
	using Extensions;

	using LinqToDB.SqlQuery.QueryElements.Enums;
	using LinqToDB.SqlQuery.QueryElements.Interfaces;
	using LinqToDB.SqlQuery.QueryElements.SqlElements;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

	using SqlProvider;
	using SqlQuery;

	class SqlCeSqlOptimizer : BasicSqlOptimizer
	{
		public SqlCeSqlOptimizer(SqlProviderFlags sqlProviderFlags) : base(sqlProviderFlags)
		{
		}

		public override ISelectQuery Finalize(ISelectQuery selectQuery)
		{
			selectQuery = base.Finalize(selectQuery);

            foreach (var parameter in QueryVisitor.FindOnce<ISqlParameter>(selectQuery.Select))
            {
                parameter.IsQueryParameter = false;
                selectQuery.IsParameterDependent = true;

            }

			switch (selectQuery.EQueryType)
			{
				case EQueryType.Delete :
					selectQuery = GetAlternativeDelete(selectQuery);
					selectQuery.From.Tables[0].Alias = "$";
					break;

				case EQueryType.Update :
					selectQuery = GetAlternativeUpdate(selectQuery);
					break;
			}

			return selectQuery;
		}

		public override IQueryExpression ConvertExpression(IQueryExpression expr)
		{
			expr = base.ConvertExpression(expr);

		    var sqlBinaryExpression = expr as ISqlBinaryExpression;
		    if (sqlBinaryExpression != null)
			{
				switch (sqlBinaryExpression.Operation)
				{
					case "%":
						return sqlBinaryExpression.Expr1.SystemType.IsIntegerType()?
							sqlBinaryExpression :
							new SqlBinaryExpression(
								typeof(int),
								new SqlFunction(typeof(int), "Convert", SqlDataType.Int32, sqlBinaryExpression.Expr1),
								sqlBinaryExpression.Operation,
								sqlBinaryExpression.Expr2,
								sqlBinaryExpression.Precedence);
				}
			}
			else
		    {
		        var sqlFunction = expr as ISqlFunction;
		        if (sqlFunction != null)
		        {
		            switch (sqlFunction.Name)
		            {
		                case "Convert" :
		                    switch (Type.GetTypeCode(sqlFunction.SystemType.ToUnderlying()))
		                    {
		                        case TypeCode.UInt64 :
		                            if (sqlFunction.Parameters[1].SystemType.IsFloatType())
		                                return new SqlFunction(
		                                    sqlFunction.SystemType,
		                                    sqlFunction.Name,
		                                    sqlFunction.Precedence,
		                                    sqlFunction.Parameters[0],
		                                    new SqlFunction(sqlFunction.SystemType, "Floor", sqlFunction.Parameters[1]));

		                            break;

		                        case TypeCode.DateTime :
		                            var type1 = sqlFunction.Parameters[1].SystemType.ToUnderlying();

		                            if (IsTimeDataType(sqlFunction.Parameters[0]))
		                            {
		                                if (type1 == typeof(DateTime) || type1 == typeof(DateTimeOffset))
		                                    return new SqlExpression(
		                                        sqlFunction.SystemType, "Cast(Convert(NChar, {0}, 114) as DateTime)", Precedence.Primary, sqlFunction.Parameters[1]);

		                                if (sqlFunction.Parameters[1].SystemType == typeof(string))
		                                    return sqlFunction.Parameters[1];

		                                return new SqlExpression(
		                                    sqlFunction.SystemType, "Convert(NChar, {0}, 114)", Precedence.Primary, sqlFunction.Parameters[1]);
		                            }

		                            if (type1 == typeof(DateTime) || type1 == typeof(DateTimeOffset))
		                            {
		                                if (IsDateDataType(sqlFunction.Parameters[0], "Datetime"))
		                                    return new SqlExpression(
		                                        sqlFunction.SystemType, "Cast(Floor(Cast({0} as Float)) as DateTime)", Precedence.Primary, sqlFunction.Parameters[1]);
		                            }

		                            break;
		                    }

		                    break;
		            }
		        }
		    }

		    return expr;
		}

	}
}
