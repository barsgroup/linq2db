using System;

namespace Bars2Db.Mapping
{
    public class InheritanceMapping
    {
        public object Code;
        public ColumnDescriptor Discriminator;
        public bool IsDefault;
        public Type Type;

        public string DiscriminatorName => Discriminator.MemberName;
    }
}