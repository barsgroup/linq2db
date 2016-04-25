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

    public class SqlExpression : BaseQueryElement,
                                 ISqlExpression
    {
        public SqlExpression(Type systemType, string expr, int precedence, params IQueryExpression[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            foreach (var value in parameters)
                if (value == null) throw new ArgumentNullException(nameof(parameters));

            SystemType = systemType;
            Expr       = expr;
            Precedence = precedence;
            Parameters = parameters;
        }

        public SqlExpression(string expr, int precedence, params IQueryExpression[] parameters)
            : this(null, expr, precedence, parameters)
        {
        }

        public SqlExpression(Type systemType, string expr, params IQueryExpression[] parameters)
            : this(systemType, expr, SqlQuery.Precedence.Unknown, parameters)
        {
        }

        public SqlExpression(string expr, params IQueryExpression[] parameters)
            : this(null, expr, SqlQuery.Precedence.Unknown, parameters)
        {
        }

        public Type             SystemType { get; }
        public string           Expr       { get; }
        public int              Precedence { get; }

        public IQueryExpression[] Parameters { get; }

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
            for (var i = 0; i < Parameters.Length; i++)
                Parameters[i] = Parameters[i].Walk(skipColumns, func);

            return func(this);
        }

        #endregion

        #region IEquatable<ISqlExpression> Members

        bool IEquatable<IQueryExpression>.Equals(IQueryExpression other)
        {
            return Equals(other, DefaultComparer);
        }

        #endregion

        #region ISqlExpression Members

        public bool CanBeNull()
        {
            foreach (var value in Parameters)
                if (value.CanBeNull())
                    return true;

            return false;
        }

        internal static Func<IQueryExpression,IQueryExpression,bool> DefaultComparer = (x, y) => true;

        public bool Equals(IQueryExpression other, Func<IQueryExpression,IQueryExpression,bool> comparer)
        {
            if (this == other)
                return true;

            var expr = other as ISqlExpression;

            if (expr == null || SystemType != expr.SystemType || Expr != expr.Expr || Parameters.Length != expr.Parameters.Length)
                return false;

            for (var i = 0; i < Parameters.Length; i++)
                if (!Parameters[i].Equals(expr.Parameters[i], comparer))
                    return false;

            return comparer(this, other);
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
                objectTree.Add(this, clone = new SqlExpression(
                    SystemType,
                    Expr,
                    Precedence,
                    Parameters.Select(e => (IQueryExpression)e.Clone(objectTree, doClone)).ToArray()));
            }

            return clone;
        }

        #endregion

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.SqlExpression;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            var len = sb.Length;
            var ss  = Parameters.Select(p =>
            {
                p.ToString(sb, dic);
                var s = sb.ToString(len, sb.Length - len);
                sb.Length = len;
                return (object)s;
            });
            
            return sb.AppendFormat(Expr, ss.ToArray());
        }

        #endregion

        #region Public Static Members

        public static bool NeedsEqual(IQueryElement ex)
        {
            switch (ex.ElementType)
            {
                case EQueryElementType.SqlParameter:
                case EQueryElementType.SqlField    :
                case EQueryElementType.Column      : return true;
                case EQueryElementType.SqlFunction :

                    var f = (ISqlFunction)ex;

                    switch (f.Name)
                    {
                        case "EXISTS" : return false;
                    }

                    return true;
            }

            return false;
        }

        #endregion
    }
}
