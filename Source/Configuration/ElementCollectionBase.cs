using System.Configuration;

namespace Bars2Db.Configuration
{
    public abstract class ElementCollectionBase<T> : ConfigurationElementCollection
        where T : ConfigurationElement, new()
    {
        public new T this[string name] => (T) BaseGet(name);

        public T this[int index] => (T) BaseGet(index);

        protected override ConfigurationElement CreateNewElement()
        {
            return new T();
        }

        protected abstract object GetElementKey(T element);

        protected sealed override object GetElementKey(ConfigurationElement element)
        {
            return GetElementKey((T) element);
        }
    }
}