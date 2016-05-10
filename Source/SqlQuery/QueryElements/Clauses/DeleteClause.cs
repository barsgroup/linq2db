using System;
using System.Collections.Generic;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Clauses.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Clauses
{
    public class DeleteClause : BaseQueryElement,
        IDeleteClause
    {
        public ISqlTable Table { get; set; }

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            var clone = new DeleteClause();

            if (Table != null)
                clone.Table = (ISqlTable) Table.Clone(objectTree, doClone);

            objectTree.Add(this, clone);

            return clone;
        }

        #endregion

        #region ISqlExpressionWalkable Members

        [Obsolete]
        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            if (Table != null)
                Table.Walk(skipColumns, func);

            return null;
        }

        #endregion

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.DeleteClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            sb.Append("DELETE FROM ");

            Table?.ToString(sb, dic);

            sb.AppendLine();

            return sb;
        }

        #endregion
    }
}