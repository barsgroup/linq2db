using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bars2Db.Common;
using Bars2Db.Extensions;

namespace Bars2Db.Reflection
{
    public class TypeAccessor<T> : TypeAccessor
    {
        private static readonly List<MemberInfo> _members = new List<MemberInfo>();
        private static readonly IObjectFactory _objectFactory;

        private static readonly Func<T> _createInstance;

        static TypeAccessor()
        {
            // Create Instance.
            //
            var type = typeof(T);

            if (type.IsValueTypeEx())
            {
                _createInstance = () => default(T);
            }
            else
            {
                var ctor = type.IsAbstractEx() ? null : type.GetDefaultConstructorEx();

                if (ctor == null)
                {
                    Expression<Func<T>> mi;

                    if (type.IsAbstractEx()) mi = () => ThrowAbstractException();
                    else mi = () => ThrowException();

                    var body = Expression.Call(null, ((MethodCallExpression) mi.Body).Method);

                    _createInstance = Expression.Lambda<Func<T>>(body).Compile();
                }
                else
                {
                    _createInstance = Expression.Lambda<Func<T>>(Expression.New(ctor)).Compile();
                }
            }

            _members.AddRange(type.GetPublicInstanceValueMembers());

            // Add explicit iterface implementation properties support
            // Or maybe we should support all private fields/properties?
            //
            var interfaceMethods =
                type.GetInterfacesEx().SelectMany(ti => type.GetInterfaceMapEx(ti).TargetMethods).ToList();

            if (interfaceMethods.Count > 0)
            {
                foreach (var pi in type.GetNonPublicPropertiesEx())
                {
                    if (pi.GetIndexParameters().Length == 0)
                    {
                        var getMethod = pi.GetGetMethodEx(true);
                        var setMethod = pi.GetSetMethodEx(true);

                        if ((getMethod == null || interfaceMethods.Contains(getMethod)) &&
                            (setMethod == null || interfaceMethods.Contains(setMethod)))
                        {
                            _members.Add(pi);
                        }
                    }
                }
            }

            // ObjectFactory
            //
            var attr = type.GetFirstAttribute<ObjectFactoryAttribute>();

            if (attr != null)
                _objectFactory = attr.ObjectFactory;
        }

        public TypeAccessor()
        {
            foreach (var member in _members)
                AddMember(new MemberAccessor(this, member));

            ObjectFactory = _objectFactory;
        }

        public override Type Type => typeof(T);

        private static T ThrowException()
        {
            throw new LinqToDBException("The '{0}' type must have default or init constructor.".Args(typeof(T).FullName));
        }

        private static T ThrowAbstractException()
        {
            throw new LinqToDBException("Cant create an instance of abstract class '{0}'.".Args(typeof(T).FullName));
        }

        public override object CreateInstance()
        {
            return _createInstance();
        }

        public T Create()
        {
            return _createInstance();
        }
    }
}