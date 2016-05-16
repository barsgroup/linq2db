using System;
using Bars2Db.Common;

namespace Bars2Db.Reflection
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ObjectFactoryAttribute : Attribute
    {
        public ObjectFactoryAttribute(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            ObjectFactory = Activator.CreateInstance(type) as IObjectFactory;

            if (ObjectFactory == null)
                throw new ArgumentException("Type '{0}' does not implement IObjectFactory interface.".Args(type));
        }

        public IObjectFactory ObjectFactory { get; }
    }
}