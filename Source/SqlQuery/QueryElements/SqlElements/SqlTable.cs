using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Bars2Db.Extensions;
using Bars2Db.Mapping;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Enums;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.SqlElements
{
    public class SqlTable<TEntity> : SqlTable
    {
        public SqlTable(MappingSchema mappingSchema)
            : base(mappingSchema, typeof(TEntity))
        {
        }
    }

    public class SqlTable : BaseQueryElement,
        ISqlTable
    {
        #region Init

        public SqlTable()
        {
            SourceID = Interlocked.Increment(ref SelectQuery.SourceIDCounter);
            Fields = new Dictionary<string, ISqlField>();
        }

        #endregion

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
            {
                var table = new SqlTable
                {
                    Name = Name,
                    Alias = Alias,
                    Database = Database,
                    Owner = Owner,
                    PhysicalName = PhysicalName,
                    ObjectType = ObjectType,
                    SqlTableType = SqlTableType,
                    SequenceAttributes = SequenceAttributes
                };

                table.Fields.Clear();

                foreach (var field in Fields)
                {
                    var fc = new SqlField(field.Value);

                    objectTree.Add(field.Value, fc);
                    table.Add(fc);
                }

                if (TableArguments != null)
                {
                    foreach (
                        var tableArgument in TableArguments.Select(e => (IQueryExpression) e.Clone(objectTree, doClone))
                        )
                    {
                        TableArguments.AddLast(tableArgument);
                    }
                }
                objectTree.Add(this, table);

                clone = table;
            }

            return clone;
        }

        #endregion

        public override EQueryElementType ElementType => EQueryElementType.SqlTable;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            return sb.Append(Name);
        }

        #region IEquatable<ISqlExpression> Members

        bool IEquatable<IQueryExpression>.Equals(IQueryExpression other)
        {
            return this == other;
        }

        #endregion

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            TableArguments.ForEach(
                node => { node.Value = node.Value.Walk(skipColumns, func); });

            return func(this);
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

        #region Init from type

        public SqlTable([Properties.NotNull] MappingSchema mappingSchema, Type objectType) : this()
        {
            if (mappingSchema == null) throw new ArgumentNullException(nameof(mappingSchema));

            var ed = mappingSchema.GetEntityDescriptor(objectType);

            Database = ed.DatabaseName;
            Owner = ed.SchemaName;
            Name = ed.TableName;
            ObjectType = objectType;
            PhysicalName = Name;

            foreach (var column in ed.Columns)
            {
                var field = new SqlField
                {
                    SystemType = column.MemberType,
                    Name = column.MemberName,
                    PhysicalName = column.ColumnName,
                    Nullable = column.CanBeNull,
                    IsPrimaryKey = column.IsPrimaryKey,
                    PrimaryKeyOrder = column.PrimaryKeyOrder,
                    IsIdentity = column.IsIdentity,
                    IsInsertable = !column.SkipOnInsert,
                    IsUpdatable = !column.SkipOnUpdate,
                    DataType = column.DataType,
                    DbType = column.DbType,
                    Length = column.Length,
                    Precision = column.Precision,
                    Scale = column.Scale,
                    CreateFormat = column.CreateFormat,
                    ColumnDescriptor = column
                };

                Add(field);

                if (field.DataType == DataType.Undefined)
                {
                    var dataType = mappingSchema.GetDataType(field.SystemType);

                    if (dataType.DataType == DataType.Undefined)
                    {
                        var canBeNull = field.Nullable;

                        dataType = mappingSchema.GetUnderlyingDataType(field.SystemType, ref canBeNull);

                        field.Nullable = canBeNull;
                    }

                    field.DataType = dataType.DataType;

                    if (field.Length == null)
                        field.Length = dataType.Length;
                }
            }

            var identityField = GetIdentityField();

            if (identityField != null)
            {
                var cd = ed[identityField.Name];

                SequenceAttributes = mappingSchema.GetAttributes<SequenceNameAttribute>(
                    cd.MemberAccessor.MemberInfo, a => a.Configuration);
            }
        }

        public SqlTable(Type objectType)
            : this(MappingSchema.Default, objectType)
        {
        }

        #endregion

        #region Init from Table

        public SqlTable(ISqlTable table) : this()
        {
            Alias = table.Alias;
            Database = table.Database;
            Owner = table.Owner;
            Name = table.Name;
            PhysicalName = table.PhysicalName;
            ObjectType = table.ObjectType;
            SequenceAttributes = table.SequenceAttributes;

            foreach (var field in table.Fields.Values)
                Add(new SqlField(field));

            SqlTableType = table.SqlTableType;
            TableArguments = table.TableArguments;
        }

        public SqlTable(ISqlTable table, IEnumerable<ISqlField> fields, LinkedList<IQueryExpression> tableArguments)
            : this()
        {
            Alias = table.Alias;
            Database = table.Database;
            Owner = table.Owner;
            Name = table.Name;
            PhysicalName = table.PhysicalName;
            ObjectType = table.ObjectType;
            SequenceAttributes = table.SequenceAttributes;

            AddRange(fields);

            SqlTableType = table.SqlTableType;
            TableArguments = tableArguments;
        }

        #endregion

        #region Public Members

        public ISqlField this[string fieldName]
        {
            get
            {
                ISqlField field;
                Fields.TryGetValue(fieldName, out field);
                return field;
            }
        }

        public string Name { get; set; }
        public string Alias { get; set; }
        public string Database { get; set; }
        public string Owner { get; set; }
        public Type ObjectType { get; set; }
        public string PhysicalName { get; set; }
        public ESqlTableType SqlTableType { get; set; }

        public LinkedList<IQueryExpression> TableArguments { get; } = new LinkedList<IQueryExpression>();

        public Dictionary<string, ISqlField> Fields { get; }

        public SequenceNameAttribute[] SequenceAttributes { get; private set; }

       
        public ISqlField GetIdentityField()
        {
            foreach (var field in Fields)
                if (field.Value.IsIdentity)
                    return field.Value;

            var keys = GetKeys(true);

            if (keys != null && keys.Count == 1)
                return (ISqlField) keys[0];

            return null;
        }

        public void Add(ISqlField field)
        {
            if (field.Table != null) throw new InvalidOperationException("Invalid parent table.");

            field.Table = this;

            Fields.Add(field.Name, field);
        }

        public void AddRange(IEnumerable<ISqlField> collection)
        {
            foreach (var item in collection)
                Add(item);
        }

        #endregion

        #region ISqlTableSource Members

        public int SourceID { get; }

        private List<IQueryExpression> _keyFields;

        public IList<IQueryExpression> GetKeys(bool allIfEmpty)
        {
            if (_keyFields == null)
            {
                _keyFields = (
                    from f in Fields.Values
                    where f.IsPrimaryKey
                    orderby f.PrimaryKeyOrder
                    select f as IQueryExpression
                    ).ToList();
            }

            if (_keyFields.Count == 0 && allIfEmpty)
                return Fields.Values.Select(f => f as IQueryExpression).ToList();

            return _keyFields;
        }

        #endregion

        #region ISqlExpression Members

        public bool CanBeNull()
        {
            return true;
        }

        public bool Equals(IQueryExpression other, Func<IQueryExpression, IQueryExpression, bool> comparer)
        {
            return this == other;
        }

        public int Precedence => SqlQuery.Precedence.Unknown;

        Type IQueryExpression.SystemType => ObjectType;

        #endregion
    }
}