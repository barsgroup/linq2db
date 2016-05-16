using System;
using System.Collections.Generic;
using System.Text;
using Bars2Db.Mapping;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.SqlElements
{
    public class SqlField : BaseQueryElement,
        ISqlField
    {
        private string _physicalName;

        public SqlField()
        {
            Nullable = true;
        }

        public SqlField(ISqlField field)
        {
            SystemType = field.SystemType;
            Alias = field.Alias;
            Name = field.Name;
            PhysicalName = field.PhysicalName;
            Nullable = field.Nullable;
            IsPrimaryKey = field.IsPrimaryKey;
            PrimaryKeyOrder = field.PrimaryKeyOrder;
            IsIdentity = field.IsIdentity;
            IsInsertable = field.IsInsertable;
            IsUpdatable = field.IsUpdatable;
            DataType = field.DataType;
            DbType = field.DbType;
            Length = field.Length;
            Precision = field.Precision;
            Scale = field.Scale;
            CreateFormat = field.CreateFormat;
            ColumnDescriptor = field.ColumnDescriptor;
        }

        public Type SystemType { get; set; }
        public string Alias { get; set; }
        public string Name { get; set; }
        public bool Nullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public int PrimaryKeyOrder { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsInsertable { get; set; }
        public bool IsUpdatable { get; set; }
        public DataType DataType { get; set; }
        public string DbType { get; set; }
        public int? Length { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public string CreateFormat { get; set; }

        public ISqlTableSource Table { get; set; }
        public ColumnDescriptor ColumnDescriptor { get; set; }

        public string PhysicalName
        {
            get { return _physicalName ?? Name; }
            set { _physicalName = value; }
        }

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            return func(this);
        }

        #endregion

        #region IEquatable<ISqlExpression> Members

        bool IEquatable<IQueryExpression>.Equals(IQueryExpression other)
        {
            return this == other;
        }

        #endregion

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            Table.Clone(objectTree, doClone);
            return objectTree[this];
        }

        #endregion

        #region Overrides

#if OVERRIDETOSTRING

        public override string ToString()
        {
            return
                ((IQueryElement) this).ToString(new StringBuilder(), new Dictionary<IQueryElement, IQueryElement>())
                    .ToString();
        }

#endif

        #endregion

        #region ISqlExpression Members

        public bool CanBeNull()
        {
            return Nullable;
        }

        public bool Equals(IQueryExpression other, Func<IQueryExpression, IQueryExpression, bool> comparer)
        {
            return this == other;
        }

        public int Precedence => SqlQuery.Precedence.Primary;

        #endregion

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.SqlField;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            return sb
                .Append('t')
                .Append(Table.SourceID)
                .Append('.')
                .Append(Name);
        }

        #endregion
    }
}