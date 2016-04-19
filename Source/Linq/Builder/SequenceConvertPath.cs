using System.Diagnostics;
using System.Linq.Expressions;

namespace LinqToDB.Linq.Builder
{
    using LinqToDB.Properties;

    [DebuggerDisplay("Path = {Path}, Expr = {Expr}, Level = {Level}")]
    public class SequenceConvertPath
    {
        [NotNull] public Expression Path;
        [NotNull] public Expression Expr;
                  public int        Level;
    }
}
