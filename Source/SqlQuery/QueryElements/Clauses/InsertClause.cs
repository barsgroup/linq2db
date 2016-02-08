namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class InsertClause : BaseQueryElement, ISqlExpressionWalkable, ICloneableElement
    {
        public InsertClause()
        {
            Items = new List<SetExpression>();
        }

        public List<SetExpression> Items        { get; private set; }
        public SqlTable            Into         { get; set; }
        public bool                WithIdentity { get; set; }

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            var clone = new InsertClause { WithIdentity = WithIdentity };

            if (Into != null)
                clone.Into = (SqlTable)Into.Clone(objectTree, doClone);

            foreach (var item in Items)
                clone.Items.Add((SetExpression)item.Clone(objectTree, doClone));

            objectTree.Add(this, clone);

            return clone;
        }

        #endregion

        #region ISqlExpressionWalkable Members

        ISqlExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            if (Into != null)
                ((ISqlExpressionWalkable)Into).Walk(skipColumns, func);

            foreach (var t in Items)
                ((ISqlExpressionWalkable)t).Walk(skipColumns, func);

            return null;
        }

        #endregion

        #region IQueryElement Members

        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
            list.Add(Into);
            list.AddRange(Items);
        }

        public override QueryElementType ElementType => QueryElementType.InsertClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            sb.Append("VALUES ");

            if (Into != null)
                ((IQueryElement)Into).ToString(sb, dic);

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