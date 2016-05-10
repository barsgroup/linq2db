using System;
using System.Collections.Generic;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Predicates
{
    public abstract class Predicate : BaseQueryElement,
        ISqlPredicate
    {
        protected Predicate(int precedence)
        {
            Precedence = precedence;
        }

        // { expression { = | <> | != | > | >= | ! > | < | <= | !< } expression
        //

        // string_expression [ NOT ] LIKE string_expression [ ESCAPE 'escape_character' ]
        //

        // expression [ NOT ] BETWEEN expression AND expression
        //

        // expression IS [ NOT ] NULL
        //

        // expression [ NOT ] IN ( subquery | expression [ ,...n ] )
        //

        // CONTAINS ( { column | * } , '< contains_search_condition >' )
        // FREETEXT ( { column | * } , 'freetext_string' )
        // expression { = | <> | != | > | >= | !> | < | <= | !< } { ALL | SOME | ANY } ( subquery )
        // EXISTS ( subquery )

        #region Overrides

#if OVERRIDETOSTRING

        public override string ToString()
        {
            return
                ((IQueryElement) this).ToString(new StringBuilder(), new Dictionary<IQueryElement, IQueryElement>())
                    .ToString();
        }

#endif

        #endregion

        #region IPredicate Members

        public int Precedence { get; }

        public abstract bool CanBeNull();

        protected abstract ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone);

        protected abstract void Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> action);

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            Walk(skipColumns, func);
            return null;
        }

        ICloneableElement ICloneableElement.Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            return Clone(objectTree, doClone);
        }

        #endregion

        #region IQueryElement Members

        protected abstract void ToStringInternal(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic);

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            if (dic.ContainsKey(this))
                return sb.Append("...");

            dic.Add(this, this);
            ToStringInternal(sb, dic);
            dic.Remove(this);

            return sb;
        }

        #endregion
    }
}