using Bars2Db.Linq;
using Bars2Db.Linq.Interfaces;

namespace Bars2Db
{
    public interface ITable<
#if !SL4
        out
#endif
            T> : IExpressionQuery<T>
    {
    }
}