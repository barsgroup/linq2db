namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class InsertClause : BaseQueryElement,
                                IInsertClause
    {
        public InsertClause()
        {
            Items = new List<ISetExpression>();
        }

        public List<ISetExpression> Items        { get; private set; }
        public ISqlTable             Into         { get; set; }
        public bool                WithIdentity { get; set; }

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            var clone = new InsertClause { WithIdentity = WithIdentity };

            if (Into != null)
                clone.Into = (ISqlTable)Into.Clone(objectTree, doClone);

            foreach (var item in Items)
                clone.Items.Add((ISetExpression)item.Clone(objectTree, doClone));

            objectTree.Add(this, clone);

            return clone;
        }

        #endregion

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> func)
        {
            if (Into != null)
                Into.Walk(skipColumns, func);

            foreach (var t in Items)
                t.Walk(skipColumns, func);

            return null;
        }

        #endregion

        #region IQueryElement Members

        public override void GetChildren(LinkedList<IQueryElement> list)
        {
            list.AddLast(Into);
            FillList(Items, list);
        }

        public override EQueryElementType ElementType => EQueryElementType.InsertClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
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