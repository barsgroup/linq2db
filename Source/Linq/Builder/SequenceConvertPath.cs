using System.Diagnostics;
using System.Linq.Expressions;
using Bars2Db.Properties;

namespace Bars2Db.Linq.Builder
{
    [DebuggerDisplay("Path = {Path}, Expr = {Expr}, Level = {Level}")]
    public class SequenceConvertPath
    {
        [NotNull] public Expression Expr;
        public int Level;
        [NotNull] public Expression Path;
    }
}