namespace LinqToDB.SqlEntities
{
    using System;
    using System.Linq;
    using System.Reflection;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    partial class Sql
	{
		[Serializable]
		[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
		public class FunctionAttribute : Attribute
		{
			public FunctionAttribute()
			{
			}

			public FunctionAttribute(string name)
			{
				Name = name;
			}

			public FunctionAttribute(string name, params int[] argIndices)
			{
				Name       = name;
				ArgIndices = argIndices;
			}

			public FunctionAttribute(string configuration, string name)
			{
				Configuration = configuration;
				Name          = name;
			}

			public FunctionAttribute(string configuration, string name, params int[] argIndices)
			{
				Configuration = configuration;
				Name          = name;
				ArgIndices    = argIndices;
			}

			public string Configuration    { get; set; }
			public string Name             { get; set; }
			public bool   ServerSideOnly   { get; set; }
			public bool   PreferServerSide { get; set; }
			public bool   InlineParameters { get; set; }
			public int[]  ArgIndices       { get; set; }

			protected IQueryExpression[] ConvertArgs(MemberInfo member, IQueryExpression[] args)
			{
				if (member is MethodInfo)
				{
					var method = (MethodInfo)member;

					if (method.DeclaringType.IsGenericTypeEx())
						args = args.Concat(method.DeclaringType.GetGenericArgumentsEx().Select(t => (IQueryExpression)SqlDataType.GetDataType(t))).ToArray();

					if (method.IsGenericMethod)
						args = args.Concat(method.GetGenericArguments().Select(t => (IQueryExpression)SqlDataType.GetDataType(t))).ToArray();
				}

				if (ArgIndices != null)
				{
					var idxs = new IQueryExpression[ArgIndices.Length];

					for (var i = 0; i < ArgIndices.Length; i++)
						idxs[i] = args[ArgIndices[i]];

					return idxs;
				}

				return args;
			}

			public virtual IQueryExpression GetExpression(MemberInfo member, params IQueryExpression[] args)
			{
				return new SqlFunction(member.GetMemberType(), Name ?? member.Name, ConvertArgs(member, args));
			}
		}
	}
}
