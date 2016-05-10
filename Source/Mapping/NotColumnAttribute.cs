using System;

namespace Bars2Db.Mapping
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class NotColumnAttribute : ColumnAttribute
    {
        public NotColumnAttribute()
        {
            IsColumn = false;
        }
    }
}