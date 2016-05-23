using System.Linq.Expressions;
using Bars2Db.Mapping;

namespace Bars2Db.Linq.Interfaces
{
    public interface IExpressionInfo
    {
        LambdaExpression GetExpression(MappingSchema mappingSchema);
    }
}