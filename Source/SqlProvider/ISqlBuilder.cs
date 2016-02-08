using System.Collections.Generic;
using System.Data;
using System.Text;

namespace LinqToDB.SqlProvider
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ISqlBuilder
	{
		int              CommandCount         (ISelectQuery selectQuery);
		void             BuildSql             (int commandNumber, ISelectQuery selectQuery, StringBuilder sb);

		StringBuilder    BuildTableName       (StringBuilder sb, string database, string owner, string table);
		object           Convert              (object value, ConvertType convertType);
		ISqlExpression   GetIdentityExpression(SqlTable table);

		StringBuilder    PrintParameters      (StringBuilder sb, IDbDataParameter[] parameters);

		StringBuilder    ReplaceParameters     (StringBuilder sb, IDbDataParameter[] parameters);

		string           ApplyQueryHints      (string sqlText, List<string> queryHints);

		string           Name { get; }
	}
}
