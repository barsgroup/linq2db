namespace LinqToDB.SqlEntities
{
    using System;
    using System.Reflection;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    partial class Sql
	{
		[Serializable]
		[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
		public class ExpressionAttribute : FunctionAttribute
		{
			public ExpressionAttribute(string expression)
				: base(expression)
			{
				Precedence = SqlQuery.Precedence.Primary;
			}

			public ExpressionAttribute(string expression, params int[] argIndices)
				: base(expression, argIndices)
			{
				Precedence = SqlQuery.Precedence.Primary;
			}

			public ExpressionAttribute(string sqlProvider, string expression)
				: base(sqlProvider, expression)
			{
				Precedence = SqlQuery.Precedence.Primary;
			}

			public ExpressionAttribute(string sqlProvider, string expression, params int[] argIndices)
				: base(sqlProvider, expression, argIndices)
			{
				Precedence = SqlQuery.Precedence.Primary;
			}

			protected new string Name
			{
				get { return base.Name; }
			}

			public string Expression
			{
				get { return base.Name;  }
				set { base.Name = value; }
			}

			public int Precedence { get; set; }

			public override IQueryExpression GetExpression(MemberInfo member, params IQueryExpression[] args)
			{
				return new SqlExpression(member.GetMemberType(), Expression ?? member.Name, Precedence, ConvertArgs(member, args));
			}
		}
	}
}
