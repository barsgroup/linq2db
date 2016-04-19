namespace LinqToDB.Extensions
{
    using System;
    using System.Collections.Generic;

    public class TypeSwitch<TResult>
    {
        private readonly Dictionary<Type, Func<object, TResult>> _matches = new Dictionary<Type, Func<object, TResult>>();

        private Func<object, TResult> _default;

        public TypeSwitch<TResult> Case<TParameter>(Func<TParameter, TResult> action)
        {
            _matches.Add(typeof(TParameter), x => action((TParameter)x));
            return this;
        }

        public TypeSwitch<TResult> Default<TParameter>(Func<TParameter, TResult> action)
        {
            _default = x => action((TParameter)x);
            return this;
        }

        public TResult Switch(object x)
        {
            Type objType = x is Type ? typeof(Type) : x.GetType();

            Func<object, TResult> action;
            if (_matches.TryGetValue(objType, out action))
            {
                return action(x);
            }

            if (_default != null)
            {
                return _default(x);
            }

            throw new ArgumentException("action");
        }
    }
}