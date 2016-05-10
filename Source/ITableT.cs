using Bars2Db.Linq;

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