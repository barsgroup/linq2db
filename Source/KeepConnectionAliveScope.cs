using System;

namespace Bars2Db
{
    public class KeepConnectionAliveScope : IDisposable
    {
        private readonly DataContext _dataContext;
        private readonly bool _savedValue;

        public KeepConnectionAliveScope(DataContext dataContext)
        {
            _dataContext = dataContext;
            _savedValue = dataContext.KeepConnectionAlive;

            dataContext.KeepConnectionAlive = true;
        }

        public void Dispose()
        {
            _dataContext.KeepConnectionAlive = _savedValue;
        }
    }
}