using System.Collections.Generic;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Visitors.Entities;

namespace Bars2Db.Linq.Joiner.Interfaces
{
    public interface IFullPathVisitor
    {
        IEnumerable<FullPathInfo> BuildFullPaths(Expression expressionNode);
    }
}