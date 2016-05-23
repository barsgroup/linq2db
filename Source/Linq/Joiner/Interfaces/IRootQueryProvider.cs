using System;
using System.Linq;

namespace Bars2Db.Linq.Joiner.Interfaces
{
    /// <summary>Провайдер данных для рутов в SmartQuery</summary>
    public interface IRootQueryProvider
    {
        IQueryable GetQuery(Type type);
    }
}