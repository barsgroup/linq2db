using System;

namespace LinqToDB.Data
{
    using LinqToDB.Properties;

    public class DataConnectionTransaction : IDisposable
    {
        public DataConnectionTransaction([NotNull] DataConnection dataConnection)
        {
            if (dataConnection == null) throw new ArgumentNullException(nameof(dataConnection));

            DataConnection = dataConnection;
        }

        public DataConnection DataConnection { get; private set; }

        bool _disposeTransaction = true;

        public void Commit()
        {
            DataConnection.CommitTransaction();
            _disposeTransaction = false;
        }

        public void Rollback()
        {
            DataConnection.RollbackTransaction();
            _disposeTransaction = false;
        }

        public void Dispose()
        {
            if (_disposeTransaction)
                DataConnection.RollbackTransaction();
        }
    }
}
