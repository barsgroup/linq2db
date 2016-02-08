namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class UpdateClause : BaseQueryElement, IQueryElement, ISqlExpressionWalkable, ICloneableElement
    {
        public UpdateClause()
        {
            Items = new List<SetExpression>();
            Keys  = new List<SetExpression>();
        }

        public List<SetExpression> Items { get; private set; }
        public List<SetExpression> Keys  { get; private set; }
        public SqlTable            Table { get; set; }

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            var clone = new UpdateClause();

            if (Table != null)
                clone.Table = (SqlTable)Table.Clone(objectTree, doClone);

            foreach (var item in Items)
                clone.Items.Add((SetExpression)item.Clone(objectTree, doClone));

            foreach (var item in Keys)
                clone.Keys.Add((SetExpression)item.Clone(objectTree, doClone));

            objectTree.Add(this, clone);

            return clone;
        }

        #endregion

        #region ISqlExpressionWalkable Members

        ISqlExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            if (Table != null)
                ((ISqlExpressionWalkable)Table).Walk(skipColumns, func);

            foreach (var t in Items)
                ((ISqlExpressionWalkable)t).Walk(skipColumns, func);

            foreach (var t in Keys)
                ((ISqlExpressionWalkable)t).Walk(skipColumns, func);

            return null;
        }

        #endregion

        #region IQueryElement Members

        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
            list.Add(Table);
            list.AddRange(Items);
            list.AddRange(Keys);
        }

        public override EQueryElementType ElementType => EQueryElementType.UpdateClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            sb.Append("SET ");

            if (Table != null)
                ((IQueryElement)Table).ToString(sb, dic);

            sb.AppendLine();

            foreach (var e in Items)
            {
                sb.Append("\t");
                ((IQueryElement)e).ToString(sb, dic);
                sb.AppendLine();
            }

            return sb;
        }

        #endregion
    }
}