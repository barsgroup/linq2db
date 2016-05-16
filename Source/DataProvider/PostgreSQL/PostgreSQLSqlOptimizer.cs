using Bars2Db.Extensions;
using Bars2Db.SqlProvider;
using Bars2Db.SqlQuery;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.DataProvider.PostgreSQL
{
    internal class PostgreSQLSqlOptimizer : BasicSqlOptimizer
    {
        public PostgreSQLSqlOptimizer(SqlProviderFlags sqlProviderFlags) : base(sqlProviderFlags)
        {
        }

        public override ISelectQuery Finalize(ISelectQuery selectQuery)
        {
            CheckAliases(selectQuery, int.MaxValue);

            selectQuery = base.Finalize(selectQuery);

            switch (selectQuery.EQueryType)
            {
                case EQueryType.Delete:
                    return GetAlternativeDelete(selectQuery);
                case EQueryType.Update:
                    return GetAlternativeUpdate(selectQuery);
                default:
                    return selectQuery;
            }
        }

        public override IQueryExpression ConvertExpression(IQueryExpression expr)
        {
            expr = base.ConvertExpression(expr);

            var sqlBinaryExpression = expr as ISqlBinaryExpression;
            if (sqlBinaryExpression != null)
            {
                switch (sqlBinaryExpression.Operation)
                {
                    case "^":
                        return new SqlBinaryExpression(sqlBinaryExpression.SystemType, sqlBinaryExpression.Expr1, "#",
                            sqlBinaryExpression.Expr2);
                    case "+":
                        return sqlBinaryExpression.SystemType == typeof(string)
                            ? new SqlBinaryExpression(sqlBinaryExpression.SystemType, sqlBinaryExpression.Expr1, "||",
                                sqlBinaryExpression.Expr2, sqlBinaryExpression.Precedence)
                            : expr;
                }
            }
            else
            {
                var sqlFunction = expr as ISqlFunction;
                if (sqlFunction != null)
                {
                    switch (sqlFunction.Name)
                    {
                        case "Convert":
                            if (sqlFunction.SystemType.ToUnderlying() == typeof(bool))
                            {
                                var ex = AlternativeConvertToBoolean(sqlFunction, 1);
                                if (ex != null)
                                    return ex;
                            }

                            return new SqlExpression(sqlFunction.SystemType, "Cast({0} as {1})", Precedence.Primary,
                                FloorBeforeConvert(sqlFunction), sqlFunction.Parameters[0]);

                        case "CharIndex":
                            return sqlFunction.Parameters.Length == 2
                                ? new SqlExpression(sqlFunction.SystemType, "Position({0} in {1})", Precedence.Primary,
                                    sqlFunction.Parameters[0], sqlFunction.Parameters[1])
                                : Add<int>(
                                    new SqlExpression(sqlFunction.SystemType, "Position({0} in {1})", Precedence.Primary,
                                        sqlFunction.Parameters[0],
                                        ConvertExpression(new SqlFunction(typeof(string), "Substring",
                                            sqlFunction.Parameters[1],
                                            sqlFunction.Parameters[2],
                                            Sub<int>(
                                                ConvertExpression(new SqlFunction(typeof(int), "Length",
                                                    sqlFunction.Parameters[1])), sqlFunction.Parameters[2])))),
                                    Sub(sqlFunction.Parameters[2], 1));
                    }
                }
                else
                {
                    var sqlExpression = expr as ISqlExpression;
                    if (sqlExpression != null)
                    {
                        if (sqlExpression.Expr.StartsWith("Extract(DOW"))
                            return
                                Inc(new SqlExpression(sqlExpression.SystemType,
                                    sqlExpression.Expr.Replace("Extract(DOW", "Extract(Dow"), sqlExpression.Parameters));

                        if (sqlExpression.Expr.StartsWith("Extract(Millisecond"))
                            return new SqlExpression(sqlExpression.SystemType, "Cast(To_Char({0}, 'MS') as int)",
                                sqlExpression.Parameters);
                    }
                }
            }

            return expr;
        }
    }
}