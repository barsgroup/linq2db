using System.Collections.Generic;
using System.Data;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlProvider
{
    public interface ISqlBuilder
    {
        string Name { get; }
        int CommandCount(ISelectQuery selectQuery);
        void BuildSql(int commandNumber, ISelectQuery selectQuery, StringBuilder sb);

        StringBuilder BuildTableName(StringBuilder sb, string database, string owner, string table);
        object Convert(object value, ConvertType convertType);
        IQueryExpression GetIdentityExpression(ISqlTable table);

        StringBuilder PrintParameters(StringBuilder sb, IDbDataParameter[] parameters);

        StringBuilder ReplaceParameters(StringBuilder sb, IDbDataParameter[] parameters);

        string ApplyQueryHints(string sqlText, List<string> queryHints);
    }
}