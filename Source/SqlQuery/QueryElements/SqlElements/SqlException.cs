namespace LinqToDB.SqlQuery.QueryElements.SqlElements
{
    using System;

    using LinqToDB.Properties;

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

