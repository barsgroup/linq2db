namespace LinqToDB.SqlEntities
{
    using System;
    using System.Reflection;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

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
