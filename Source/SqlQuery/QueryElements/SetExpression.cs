namespace LinqToDB.SqlQuery.QueryElements
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class SetExpression : BaseQueryElement, ISetExpression
    {
        public SetExpression(IQueryExpression column, IQueryExpression expression)
        {
            Column     = column;
            Expression = expression;

            //if (expression is ISqlParameter)
            //{
            //    var sqlField = column as ISqlField;
            //    if (sqlField?.ColumnDescriptor != null)
            //    {
            //        //							if (field.ColumnDescriptorptor.MapMemberInfo.IsDbTypeSet)
            //        //								p.DbType = field.ColumnDescriptorptor.MapMemberInfo.DbType;
            //        //
            //        //							if (field.ColumnDescriptorptor.MapMemberInfo.IsDbSizeSet)
            //        //								p.DbSize = field.ColumnDescriptor.MapMemberInfo.DbSize;
            //    }
            //}
        }

        public IQueryExpression Column     { get; set; }
        public IQueryExpression Expression { get; set; }

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
            {
                objectTree.Add(this, clone = new SetExpression(
                                                 (IQueryExpression)Column.    Clone(objectTree, doClone),
                                                 (IQueryExpression)Expression.Clone(objectTree, doClone)));
            }

            return clone;
        }

        #endregion

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> func)
        {
            Column     = Column.    Walk(skipColumns, func);
            Expression = Expression.Walk(skipColumns, func);
            return null;
        }

        #endregion

        #region IQueryElement Members

        public override void GetChildren(LinkedList<IQueryElement> list)
        {
            list.AddLast(Column);
            list.AddLast(Expression);
        }

        public override EQueryElementType ElementType => EQueryElementType.SetExpression;

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