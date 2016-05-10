using System.Linq.Expressions;

namespace Bars2Db.Linq
{
    internal class Table<T> : ExpressionQuery<T>, ITable<T>, ITable
    {
        public string DatabaseName;
        public string SchemaName;
        public string TableName;

        public Table(IDataContextInfo dataContextInfo)
        {
            Init(dataContextInfo, null);
        }

        public Table(IDataContext dataContext)
        {
            Init(dataContext == null ? null : new DataContextInfo(dataContext), null);
        }

        public Table(IDataContext dataContext, Expression expression)
        {
            Init(dataContext == null ? null : new DataContextInfo(dataContext), expression);
        }

#if !SILVERLIGHT

        public Table()
        {
            Init(null, null);
        }

#endif

        #region Overrides

#if OVERRIDETOSTRING

        public override string ToString()
        {
            return "Table(" + typeof(T).Name + ")";
        }

#endif

        #endregion
    }
}