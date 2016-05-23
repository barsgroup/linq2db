using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bars2Db.Linq.Joiner.Visitors.Handlers;

namespace Bars2Db.Linq.Joiner.Visitors.Entities
{
    /// <summary>Структура представляющая собой путь в дереве выражений</summary>
    public class FullPathInfo
    {
        private readonly PropertyInfo[] _propertyInfos;

        private readonly Expression _root;

        /// <summary>Expression представляющий собой IQueryable</summary>
        public Expression Root
        {
            get { return _root; }
        }

        public IQueryable MapQueryable { get; set; }

        /// <summary>Коллекция свойств используемых в пути [Oktmo.Okato.Id]</summary>
        public PropertyInfo[] PropertyInfos
        {
            get { return _propertyInfos; }
        }

        public FullPathInfo(Expression root, PropertyInfo[] propertyInfos)
        {
            _propertyInfos = propertyInfos;
            _root = root;
        }

        public FullPathInfo Copy(Expression copyRoot)
        {
            return new FullPathInfo(copyRoot, PropertyInfos);
        }

        /// <summary>Создает путь</summary>
        /// <param name="root">Expression представляющий собой IQueryable</param>
        /// <param name="path">Путь обращения к полю например(x.Okato.Oktmo.Okei)</param>
        public static FullPathInfo CreatePath(Expression root, Expression path = null)
        {
            var propertyInfos = path != null
                                    ? path.GetMembersFromChain()
                                    : new MemberInfo[] { };

            var typeIterator = root.Type.GetGenericArguments().Single();

            var resultProperties = new List<PropertyInfo>();

            foreach (var propertyInfo in propertyInfos)
            {
                var realProperty = typeIterator.GetProperty(propertyInfo.Name) ?? (PropertyInfo)propertyInfo;

                resultProperties.Add(realProperty);

                typeIterator = realProperty.PropertyType;
            }

            var result = new FullPathInfo(root, resultProperties.ToArray());

            return result;
        }

        public override bool Equals(object obj)
        {
            var right = obj as FullPathInfo;

            if (right == null)
            {
                return false;
            }

            var left = this;

            if (left.Root != right.Root)
            {
                return false;
            }

            var leftPropertyInfos = left.PropertyInfos;
            var rightPropertyInfos = right.PropertyInfos;

            if (leftPropertyInfos.Length != rightPropertyInfos.Length)
            {
                return false;
            }

            for (var i = 0; i < leftPropertyInfos.Length; i++)
            {
                var leftProperty = leftPropertyInfos[i];
                var rightProperty = rightPropertyInfos[i];

                if (leftProperty.Name != rightProperty.Name || leftProperty.PropertyType != rightProperty.PropertyType)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetDepth()
        {
            return _propertyInfos.Length;
        }

        public override int GetHashCode()
        {
            ///WTF алгоритм
            var hash = 13;
            hash = hash * 7 + _root.GetHashCode();

            foreach (var propertyInfo in _propertyInfos)
            {
                hash = hash * 7 + propertyInfo.Name.GetHashCode() + propertyInfo.PropertyType.GetHashCode();
            }

            return hash;
        }

        /// <summary>Заменяет в пути fullPath часть равную oldStartPart на newPart</summary>
        public static FullPathInfo ReplaceStartPart(FullPathInfo fullPath, FullPathInfo oldStartPart, FullPathInfo newStartPart)
        {
            var newProperties = newStartPart.PropertyInfos.Concat(fullPath.PropertyInfos.Skip(oldStartPart.PropertyInfos.Length)).ToArray();

            var result = new FullPathInfo(newStartPart.Root, newProperties);

            return result;
        }

        /// <summary>Проверяет начинается ли fullPath со startPath</summary>
        public static bool StartWith(FullPathInfo fullPath, FullPathInfo startPath)
        {
            var depthDif = fullPath.GetDepth() - startPath.GetDepth();

            if (depthDif < 0)
            {
                return false;
            }

            var sameDepthPath = new FullPathInfo(startPath.Root, fullPath.PropertyInfos.Take(startPath.GetDepth()).ToArray());

            return sameDepthPath.Equals(startPath);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var type = Root.Type.GetGenericArguments()[0];

            return string.Format("{0}; {1}", type.Name, string.Join(",", PropertyInfos.Select(x => x.Name)));
        }
    }
}