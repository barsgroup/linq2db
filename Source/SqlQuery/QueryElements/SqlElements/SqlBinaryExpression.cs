namespace LinqToDB.SqlQuery.QueryElements.SqlElements
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    [Serializable, DebuggerDisplay("SQL = {SqlText}")]
    public class SqlBinaryExpression : BaseQueryElement,
                                       ISqlBinaryExpression
    {
        public SqlBinaryExpression(Type systemType, IQueryExpression expr1, string operation, IQueryExpression expr2, int precedence = SqlQuery.Precedence.Unknown)
        {
            if (expr1     == null) throw new ArgumentNullException(nameof(expr1));
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            if (expr2     == null) throw new ArgumentNullException(nameof(expr2));

            Expr1      = expr1;
            Operation  = operation;
            Expr2      = expr2;
            SystemType = systemType;
            Precedence = precedence;
        }

        public IQueryExpression Expr1 { get; set; }

        public string         Operation  { get; }

        public IQueryExpression Expr2      { get; set; }

        public Type           SystemType { get; }

        public int            Precedence { get; }

        #region Overrides

#if OVERRIDETOSTRING

        public override string ToString()
        {
            return ((IQueryElement)this).ToString(new StringBuilder(), new Dictionary<IQueryElement,IQueryElement>()).ToString();
        }

#endif

        #endregion

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> func)
        {
            Expr1 = Expr1.Walk(skipColumns, func);
            Expr2 = Expr2.Walk(skipColumns, func);

            return func(this);
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
            return Expr1.CanBeNull() || Expr2.CanBeNull();
        }

        public bool Equals(IQueryExpression other, Func<IQueryExpression,IQueryExpression,bool> comparer)
        {
            if (this == other)
                return true;

            var expr = other as ISqlBinaryExpression;

            return
                expr        != null                &&
                Operation  == expr.Operation       &&
                SystemType == expr.SystemType      &&
                Expr1.Equals(expr.Expr1, comparer) &&
                Expr2.Equals(expr.Expr2, comparer) &&
                comparer(this, other);
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
                objectTree.Add(this, clone = new SqlBinaryExpression(
                    SystemType,
                    (IQueryExpression)Expr1.Clone(objectTree, doClone),
                    Operation,
                    (IQueryExpression)Expr2.Clone(objectTree, doClone),
                    Precedence));
            }

            return clone;
        }

        #endregion

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.SqlBinaryExpression;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            Expr1
                .ToString(sb, dic)
                .Append(' ')
                .Append(Operation)
                .Append(' ');

            return Expr2.ToString(sb, dic);
        }

        #endregion
    }
}
