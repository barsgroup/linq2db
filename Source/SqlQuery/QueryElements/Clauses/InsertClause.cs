using System;
using System.Collections.Generic;
using System.Text;
using Bars2Db.Extensions;
using Bars2Db.SqlQuery.QueryElements.Clauses.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Clauses
{
    public class InsertClause : BaseQueryElement,
        IInsertClause
    {
        public LinkedList<ISetExpression> Items { get; } = new LinkedList<ISetExpression>();

        public ISqlTable Into { get; set; }

        public bool WithIdentity { get; set; }

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            var clone = new InsertClause {WithIdentity = WithIdentity};

            if (Into != null)
                clone.Into = (ISqlTable) Into.Clone(objectTree, doClone);

            Items.ForEach(
                node => { clone.Items.AddLast((ISetExpression) node.Value.Clone(objectTree, doClone)); });

            objectTree.Add(this, clone);

            return clone;
        }

        #endregion

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            if (Into != null)
                Into.Walk(skipColumns, func);

            foreach (var t in Items)
                t.Walk(skipColumns, func);

            return null;
        }

        #endregion

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.InsertClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            sb.Append("VALUES ");

            if (Into != null)
                Into.ToString(sb, dic);

            sb.AppendLine();

            foreach (var e in Items)
            {
                sb.Append("\t");
                e.ToString(sb, dic);
                sb.AppendLine();
            }

            return sb;
        }

        #endregion
    }
}