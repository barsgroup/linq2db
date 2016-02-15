namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class UpdateClause : BaseQueryElement,
                                IUpdateClause
    {
        public UpdateClause()
        {
            Items = new List<ISetExpression>();
            Keys  = new List<ISetExpression>();
        }

        public List<ISetExpression> Items { get; private set; }
        public List<ISetExpression> Keys  { get; private set; }
        public ISqlTable Table { get; set; }

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            var clone = new UpdateClause();

            if (Table != null)
                clone.Table = (ISqlTable)Table.Clone(objectTree, doClone);

            foreach (var item in Items)
                clone.Items.Add((ISetExpression)item.Clone(objectTree, doClone));

            foreach (var item in Keys)
                clone.Keys.Add((ISetExpression)item.Clone(objectTree, doClone));

            objectTree.Add(this, clone);

            return clone;
        }

        #endregion

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> func)
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

        public override void GetChildren(LinkedList<IQueryElement> list)
        {
            list.AddLast(Table);
            FillList(Items, list);
            FillList(Keys, list);
        }

        public override EQueryElementType ElementType => EQueryElementType.UpdateClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
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