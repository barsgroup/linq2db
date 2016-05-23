using System.Linq;

namespace Bars2Db.Linq.Joiner.Interfaces
{
    public interface ISmartQuery<out TEntity> : IOrderedQueryable<TEntity>
    {
    }
}