using System;
using Bars2Db.Properties;

namespace Bars2Db.Data
{
    public class DataConnectionTransaction : IDisposable
    {
        private bool _disposeTransaction = true;

        public DataConnectionTransaction([NotNull] DataConnection dataConnection)
        {
            if (dataConnection == null) throw new ArgumentNullException(nameof(dataConnection));

            DataConnection = dataConnection;
        }

        public DataConnection DataConnection { get; }

        public void Dispose()
        {
            if (_disposeTransaction)
                DataConnection.RollbackTransaction();
        }

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
    }
}