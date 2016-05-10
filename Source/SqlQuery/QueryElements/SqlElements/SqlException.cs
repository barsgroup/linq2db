using System;
using Bars2Db.Properties;

namespace Bars2Db.SqlQuery.QueryElements.SqlElements
{
    public class SqlException : Exception
    {
        public SqlException(string message)
            : base(message)
        {
        }

        [StringFormatMethod("message")]
        public SqlException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }
    }
}