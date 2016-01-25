namespace LinqToDB.SqlQuery.QueryElements
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.SqlElements;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class SetExpression : BaseQueryElement, ISqlExpressionWalkable, ICloneableElement
    {
        public SetExpression(ISqlExpression column, ISqlExpression expression)
        {
            Column     = column;
            Expression = expression;

            if (expression is SqlParameter)
            {
                var p = (SqlParameter)expression;

                if (column is SqlField)
                {
                    var field = (SqlField)column;

                    if (field.ColumnDescriptor != null)
                    {
                        //							if (field.ColumnDescriptorptor.MapMemberInfo.IsDbTypeSet)
                        //								p.DbType = field.ColumnDescriptorptor.MapMemberInfo.DbType;
                        //
                        //							if (field.ColumnDescriptorptor.MapMemberInfo.IsDbSizeSet)
                        //								p.DbSize = field.ColumnDescriptor.MapMemberInfo.DbSize;
                    }
                }
            }
        }

        public ISqlExpression Column     { get; set; }
        public ISqlExpression Expression { get; set; }

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
            {
                objectTree.Add(this, clone = new SetExpression(
                                                 (ISqlExpression)Column.    Clone(objectTree, doClone),
                                                 (ISqlExpression)Expression.Clone(objectTree, doClone)));
            }

            return clone;
        }

        #endregion

        #region ISqlExpressionWalkable Members

        ISqlExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            Column     = Column.    Walk(skipColumns, func);
            Expression = Expression.Walk(skipColumns, func);
            return null;
        }

        #endregion

        #region IQueryElement Members

        protected override IEnumerable<IQueryElement> GetChildItemsInternal()
        {
            yield return Column;
            yield return Expression;
        }

        public override QueryElementType ElementType => QueryElementType.SetExpression;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            Column.ToString(sb, dic);
            sb.Append(" = ");
            Expression.ToString(sb, dic);

            return sb;
        }

        #endregion
    }
}