using System.Collections.Generic;

namespace Bars2Db.Extensions
{
    public static class HashSetExtentions
    {
        public static void AddRange<TElement1, TElement2>(this HashSet<TElement1> sourceList,
            LinkedList<TElement2> targetList) where TElement2 : TElement1
        {
            targetList.ForEach(node => sourceList.Add(node.Value));
        }

        public static void AddRange<TElement1, TElement2>(this HashSet<TElement1> sourceList, List<TElement2> targetList)
            where TElement2 : TElement1
        {
            for (var i = 0; i < targetList.Count; i++)
            {
                sourceList.Add(targetList[i]);
            }
        }

        public static void AddRange<TElement1, TElement2>(this HashSet<TElement1> sourceList, TElement2[] targetList)
            where TElement2 : TElement1
        {
            for (var i = 0; i < targetList.Length; i++)
            {
                sourceList.Add(targetList[i]);
            }
        }

        public static void AddRange<TElement1, TElement2>(this HashSet<TElement1> sourceList,
            IEnumerable<TElement2> targetList) where TElement2 : TElement1
        {
            foreach (var e in targetList)
            {
                sourceList.Add(e);
            }
        }
    }
}