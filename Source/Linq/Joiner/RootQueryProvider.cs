using System;
using System.Linq;
using Bars2Db.Linq.Joiner.Interfaces;

namespace Bars2Db.Linq.Joiner
{
    /// <summary>Провайдер данных для рутов в SmartQuery</summary>
    public class RootQueryProvider : IRootQueryProvider
    {
        public IQueryable GetQuery(Type type)
        {
            return null;
        }
    }
}