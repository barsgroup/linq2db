using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bars2Db.SqlQuery.Search.PathBuilder
{
    public class CompositPropertyVertex
    {
        public LinkedList<PropertyInfo> PropertyList { get; } = new LinkedList<PropertyInfo>();

        public LinkedList<CompositPropertyVertex> Children { get; } = new LinkedList<CompositPropertyVertex>();

        public override string ToString()
        {
            return string.Join("->", PropertyList.Select(p => $"{p.DeclaringType.Name}.{p.Name}"));
        }
    }
}