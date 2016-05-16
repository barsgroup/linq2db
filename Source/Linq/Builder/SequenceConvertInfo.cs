using System.Collections.Generic;
using System.Linq.Expressions;

namespace Bars2Db.Linq.Builder
{
    public class SequenceConvertInfo
    {
        public Expression Expression;
        public List<SequenceConvertPath> ExpressionsToReplace;
        public ParameterExpression Parameter;
    }
}