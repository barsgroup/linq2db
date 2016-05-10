using System;
using System.Data;
using Bars2Db.Properties;

namespace Bars2Db
{
    public class DataContextTransaction : IDisposable
    {
        private int _transactionCounter;

        public DataContextTransaction([NotNull] DataContext dataContext)
        {
            if (dataContext == null) throw new ArgumentNullException(nameof(dataContext));

            DataContext = dataContext;
        }

        public DataContext DataContext { get; set; }

        public void Dispose()
        {
            if (_transactionCounter > 0)
            {
                var db = DataContext.GetDataConnection();

                db.RollbackTransaction();

                _transactionCounter = 0;

                DataContext.LockDbManagerCounter--;
            }
        }

        public void BeginTransaction()
        {
            var db = DataContext.GetDataConnection();

            db.BeginTransaction();

            if (_transactionCounter == 0)
                DataContext.LockDbManagerCounter++;

            _transactionCounter++;
        }

        public void BeginTransaction(IsolationLevel level)
        {
            var db = DataContext.GetDataConnection();

            db.BeginTransaction(level);

            if (_transactionCounter == 0)
                DataContext.LockDbManagerCounter++;

            _transactionCounter++;
        }

        public void CommitTransaction()
        {
            if (_transactionCounter > 0)
            {
                var db = DataContext.GetDataConnection();

                db.CommitTransaction();

                _transactionCounter--;

                if (_transactionCounter == 0)
                {
                    DataContext.LockDbManagerCounter--;
                    DataContext.ReleaseQuery();
                }
            }
        }

        public void RollbackTransaction()
        {
            if (_transactionCounter > 0)
            {
                var db = DataContext.GetDataConnection();

                db.RollbackTransaction();

                _transactionCounter--;

                if (_transactionCounter == 0)
                {
                    DataContext.LockDbManagerCounter--;
                    DataContext.ReleaseQuery();
                }
            }
        }
    }
}