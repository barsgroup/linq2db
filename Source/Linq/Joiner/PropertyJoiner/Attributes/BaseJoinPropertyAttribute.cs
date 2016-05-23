using System;
using System.Configuration;
using System.Reflection;

namespace Bars2Db.Linq.Joiner.PropertyJoiner.Attributes
{
    /// <summary>
    ///     Атрибут для получения наименования свойства по которому делать join (JOIN ON JoinEntity.[JoinOn] ==
    ///     FromEntity.[JoinFrom])
    /// </summary>
    public abstract class BaseJoinPropertyAttribute : Attribute
    {
        public abstract string JoinOnPropertyName { get; }

        /// <summary>Получить свойство ключа, на которое ссылаются</summary>
        /// <param name="referencedEntity">Тип сущности, на которую ссылаются</param>
        /// <example>Oktmo => Oktmo.MetaId</example>
        public PropertyInfo GetTheirKeyProperty(Type referencedEntity)
        {
            var referencedKeyProperty = referencedEntity.GetProperty(JoinOnPropertyName);

            if (referencedKeyProperty == null)
            {
                throw new ConfigurationErrorsException(
                    string.Format("Сущность, на которую ссылается поле, отмеченное атрибутом {0}, должна иметь свойство {1}", GetType().Name, JoinOnPropertyName));
            }

            return referencedKeyProperty;
        }

        /// <summary>Получить ссылающееся свойство ключа по свойству типа сущности</summary>
        /// <param name="targetProperty">Свойство типа сущности</param>
        /// <example>Entity.Oktmo => Entity.OktmoMetaId</example>
        public PropertyInfo GetOurKeyProperty(PropertyInfo targetProperty)
        {
            var referencingKeyProperty = targetProperty.ReflectedType.GetProperty(GetOurKeyPropertyName(targetProperty));

            if (referencingKeyProperty == null)
            {
                throw new ConfigurationErrorsException(
                    string.Format("Для поля, отмеченного атрибутом {0}, должно быть соответствующее поле с суфиксом {1}", GetType().Name, JoinOnPropertyName));
            }

            return referencingKeyProperty;
        }

        /// <summary>Получить имя свойства ключа по свойству типа сущности</summary>
        /// <param name="targetProperty">Свойство типа сущности</param>
        /// <example>Entity.Oktmo => Entity.OktmoMetaId</example>
        public string GetOurKeyPropertyName(PropertyInfo targetProperty)
        {
            return targetProperty.Name + JoinOnPropertyName;
        }
    }
}