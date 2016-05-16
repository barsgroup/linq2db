using System.Collections.Generic;
using System.Reflection;
using Bars2Db.Extensions;

namespace Bars2Db.Linq
{
    internal class MemberInfoComparer : IEqualityComparer<MemberInfo>
    {
        public bool Equals(MemberInfo x, MemberInfo y)
        {
            return x.EqualsTo(y);
        }

        public int GetHashCode(MemberInfo obj)
        {
            return obj == null ? 0 : obj.Name.GetHashCode();
        }
    }
}