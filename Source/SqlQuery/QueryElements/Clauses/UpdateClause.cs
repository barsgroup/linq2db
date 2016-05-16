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
    public class UpdateClause : BaseQueryElement,
        IUpdateClause
    {
        public LinkedList<ISetExpression> Items { get; } = new LinkedList<ISetExpression>();

        public LinkedList<ISetExpression> Keys { get; } = new LinkedList<ISetExpression>();

        public ISqlTable Table { get; set; }

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            var clone = new UpdateClause();

            if (Table != null)
                clone.Table = (ISqlTable) Table.Clone(objectTree, doClone);

            Items.ForEach(node => clone.Items.AddLast((ISetExpression) node.Value.Clone(objectTree, doClone)));
            Keys.ForEach(node => clone.Keys.AddLast((ISetExpression) node.Value.Clone(objectTree, doClone)));

            objectTree.Add(this, clone);

            return clone;
        }

        #endregion

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            if (Table != null)
                Table.Walk(skipColumns, func);

            foreach (var t in Items)
                t.Walk(skipColumns, func);

            foreach (var t in Keys)
                t.Walk(skipColumns, func);

            return null;
        }

        #endregion

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.UpdateClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            sb.Append("SET ");

            if (Table != null)
                Table.ToString(sb, dic);

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