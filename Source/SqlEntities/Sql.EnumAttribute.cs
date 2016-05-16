using System;

namespace Bars2Db.SqlEntities
{
    partial class Sql
    {
        [AttributeUsage(AttributeTargets.Enum)]
        public class EnumAttribute : Attribute
        {
        }
    }
}