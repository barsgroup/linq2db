namespace LinqToDB.SqlQuery.QueryElements.SqlElements
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ISqlValue : IQueryExpression,
                                 IValueContainer
    {
    }

    public class SqlValue : BaseQueryElement,
                            ISqlValue
    {
		public SqlValue(Type systemType, object value)
		{
			SystemType = systemType;
			Value      = value;
		}

		public SqlValue(object value)
		{
			Value = value;

			if (value != null)
				SystemType = value.GetType();
		}

		public object Value      { get;  set; }
		public Type   SystemType { get; private set; }

		#region Overrides

#if OVERRIDETOSTRING

		public override string ToString()
		{
			return ((IQueryElement)this).ToString(new StringBuilder(), new Dictionary<IQueryElement,IQueryElement>()).ToString();
		}

#endif

		#endregion

		#region ISqlExpression Members

		public int Precedence
		{
			get { return SqlQuery.Precedence.Primary; }
		}

		#endregion

		#region ISqlExpressionWalkable Members

		IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> func)
		{
			return func(this);
		}

		#endregion

		#region IEquatable<ISqlExpression> Members

		bool IEquatable<IQueryExpression>.Equals(IQueryExpression other)
		{
			if (this == other)
				return true;

			var value = other as ISqlValue;
			return
				value       != null              &&
				SystemType == value.SystemType &&
				(Value == null && value.Value == null || Value != null && Value.Equals(value.Value));
		}

		#endregion

		#region ISqlExpression Members

		public bool CanBeNull()
		{
			return Value == null;
		}

		public bool Equals(IQueryExpression other, Func<IQueryExpression,IQueryExpression,bool> comparer)
		{
			return ((IQueryExpression)this).Equals(other) && comparer(this, other);
		}

		#endregion

		#region ICloneableElement Members

		public ICloneableElement Clone(Dictionary<ICloneableElement,ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
		{
			if (!doClone(this))
				return this;

			ICloneableElement clone;

			if (!objectTree.TryGetValue(this, out clone))
				objectTree.Add(this, clone = new SqlValue(SystemType, Value));

			return clone;
		}

		#endregion

		#region IQueryElement Members

        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
        }

        public override EQueryElementType ElementType { get { return EQueryElementType.SqlValue; } }

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
		{
			return 
				Value == null ?
					sb.Append("NULL") :
				Value is string ?
					sb
						.Append('\'')
						.Append(Value.ToString().Replace("\'", "''"))
						.Append('\'')
				:
					sb.Append(Value);
		}

		#endregion
	}
}
