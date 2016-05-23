using System.Linq.Expressions;
using System.Reflection;

namespace Bars2Db.Linq.Joiner.Visitors.Entities
{
    public class PropertyBindingInfo
    {
        public MemberInfo Property { get; set; }

        public Expression ValueExpression { get; set; }

        public PropertyBindingInfo(MemberInfo property, Expression valueExpression)
        {
            ValueExpression = valueExpression;
            Property = property;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Format("{0} = {1}", Property.Name, ValueExpression);
        }
    }
}