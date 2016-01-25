namespace LinqToDB.SqlQuery.QueryElements
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.SqlElements;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class CreateTableStatement : BaseQueryElement, IQueryElement, ISqlExpressionWalkable, ICloneableElement
    {
        public SqlTable       Table           { get; set; }
        public bool           IsDrop          { get; set; }
        public string         StatementHeader { get; set; }
        public string         StatementFooter { get; set; }
        public DefaulNullable DefaulNullable  { get; set; }

        #region IQueryElement Members

        protected override IEnumerable<IQueryElement> GetChildItemsInternal()
        {
            yield return Table;
        }

        public override QueryElementType ElementType => QueryElementType.CreateTableStatement;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            sb.Append(IsDrop ? "DROP TABLE " : "CREATE TABLE ");

            ((IQueryElement)Table)?.ToString(sb, dic);

            sb.AppendLine();

            return sb;
        }

        #endregion

        #region ISqlExpressionWalkable Members

        ISqlExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            ((ISqlExpressionWalkable)Table)?.Walk(skipColumns, func);

            return null;
        }

        #endregion

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement,ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            var clone = new CreateTableStatement { };

            if (Table != null)
                clone.Table = (SqlTable)Table.Clone(objectTree, doClone);

            objectTree.Add(this, clone);

            return clone;
        }

        #endregion
    }
}