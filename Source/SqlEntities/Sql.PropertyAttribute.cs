using System;
using System.Reflection;
using Bars2Db.Extensions;
using Bars2Db.SqlQuery;
using Bars2Db.SqlQuery.QueryElements.SqlElements;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlEntities
{
    partial class Sql
    {
        [Serializable]
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
        public class PropertyAttribute : FunctionAttribute
        {
            public PropertyAttribute()
            {
            }

            public PropertyAttribute(string name)
                : base(name)
            {
            }

            public PropertyAttribute(string sqlProvider, string name)
                : base(sqlProvider, name)
            {
            }

            public override IQueryExpression GetExpression(MemberInfo member, params IQueryExpression[] args)
            {
                return new SqlExpression(member.GetMemberType(), Name ?? member.Name, Precedence.Primary);
            }
        }
    }
}