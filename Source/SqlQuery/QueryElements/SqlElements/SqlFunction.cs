namespace LinqToDB.SqlQuery.QueryElements.SqlElements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public class SqlFunction : BaseQueryElement,
                               ISqlFunction

        //ISqlTableSource
    {
		public SqlFunction(Type systemType, string name, params IQueryExpression[] parameters)
			: this(systemType, name, SqlQuery.Precedence.Primary, parameters)
		{
		}

		public SqlFunction(Type systemType, string name, int precedence, params IQueryExpression[] parameters)
		{
			//_sourceID = Interlocked.Increment(ref SqlQuery.SourceIDCounter);

			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			foreach (var p in parameters)
				if (p == null) throw new ArgumentNullException(nameof(parameters));

			SystemType = systemType;
			Name       = name;
			Precedence = precedence;
			Parameters = parameters;
		}

		public Type             SystemType { get; private set; }
		public string           Name       { get; private set; }
		public int              Precedence { get; private set; }

        public IQueryExpression[] Parameters { get; private set; }

		public static ISqlFunction CreateCount (Type type, ISqlTableSource table) { return new SqlFunction(type, "Count",  new SqlExpression("*")); }

        public static ISqlFunction CreateAll   (ISelectQuery subQuery) { return new SqlFunction(typeof(bool), "ALL",    SqlQuery.Precedence.Comparison, subQuery); }
		public static ISqlFunction CreateSome  (ISelectQuery subQuery) { return new SqlFunction(typeof(bool), "SOME",   SqlQuery.Precedence.Comparison, subQuery); }
		public static ISqlFunction CreateAny   (ISelectQuery subQuery) { return new SqlFunction(typeof(bool), "ANY",    SqlQuery.Precedence.Comparison, subQuery); }
		public static ISqlFunction CreateExists(ISelectQuery subQuery) { return new SqlFunction(typeof(bool), "EXISTS", SqlQuery.Precedence.Comparison, subQuery); }

		#region Overrides

#if OVERRIDETOSTRING

		public override string ToString()
		{
			return ((IQueryElement)this).ToString(new StringBuilder(), new Dictionary<IQueryElement,IQueryElement>()).ToString();
		}

#endif

		#endregion

		#region ISqlExpressionWalkable Members

		IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> action)
		{
			for (var i = 0; i < Parameters.Length; i++)
				Parameters[i] = Parameters[i].Walk(skipColumns, action);

			return action(this);
		}

		#endregion

		#region IEquatable<ISqlExpression> Members

		bool IEquatable<IQueryExpression>.Equals(IQueryExpression other)
		{
			return Equals(other, SqlExpression.DefaultComparer);
		}

		#endregion

		#region ISqlExpression Members

		public bool CanBeNull()
		{
			return true;
		}

		#endregion

		#region ICloneableElement Members

		public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
		{
			if (!doClone(this))
				return this;

			ICloneableElement clone;

			if (!objectTree.TryGetValue(this, out clone))
			{
				objectTree.Add(this, clone = new SqlFunction(
					SystemType,
					Name,
					Precedence,
					Parameters.Select(e => (IQueryExpression)e.Clone(objectTree, doClone)).ToArray()));
			}

			return clone;
		}

		public bool Equals(IQueryExpression other, Func<IQueryExpression,IQueryExpression,bool> comparer)
		{
			if (this == other)
				return true;

			var func = other as ISqlFunction;

			if (func == null || Name != func.Name || Parameters.Length != func.Parameters.Length && SystemType != func.SystemType)
				return false;

			for (var i = 0; i < Parameters.Length; i++)
				if (!Parameters[i].Equals(func.Parameters[i], comparer))
					return false;

			return comparer(this, other);
		}

        #endregion

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.SqlFunction;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
		{
			sb
				.Append(Name)
				.Append("(");

			foreach (var p in Parameters)
			{
				p.ToString(sb, dic);
				sb.Append(", ");
			}

			if (Parameters.Length > 0)
				sb.Length -= 2;

			return sb.Append(")");
		}

		#endregion
	}
}
