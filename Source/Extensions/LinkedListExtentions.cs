namespace LinqToDB.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public static class LinkedListExtentions
    {
        [DebuggerHidden]
        public static void ForEach<TElement>(this LinkedList<TElement> linkedList, Action<LinkedListNode<TElement>> action)
        {
            var current = linkedList.First;
            while (current != null)
            {
                var next = current.Next;

                action(current);

                current = next;
            }
        }

        public static void AddRange<TElement1, TElement2>(this LinkedList<TElement1> sourceList, LinkedList<TElement2> targetList) where TElement2 : TElement1
        {
            targetList.ForEach(node => sourceList.AddLast(node.Value));
        }

        public static void AddRange<TElement1, TElement2>(this LinkedList<TElement1> sourceList, List<TElement2> targetList) where TElement2: TElement1
        {
            for (int i = 0; i < targetList.Count; i++)
            {
                sourceList.AddLast(targetList[i]);
            }
        }

        public static void AddRange<TElement1, TElement2>(this LinkedList<TElement1> sourceList, TElement2[] targetList) where TElement2 : TElement1
        {
            for (int i = 0; i < targetList.Length; i++)
            {
                sourceList.AddLast(targetList[i]);
            }
        }

        public static TResult FindOnce<TElement, TResult>(this LinkedList<TElement> linkedList, Func<LinkedListNode<TElement>, TResult> action)
        {
            var current = linkedList.First;
            while (current != null)
            {
                var next = current.Next;
                var result = action(current);
                if (!Equals(result, default(TResult)))
                {
                    return result;
                }

                current = next;
            }

            return default(TResult);
        }


        public static void Each<TElement>(this LinkedListNode<TElement> node, Action<LinkedListNode<TElement>> action)
        {
            var current = node;
            while (current != null)
            {
                var previous = current.Next;

                action(current);

                current = previous;
            }
        }

        public static void ReverseEach<TElement>(this LinkedListNode<TElement> node, Action<LinkedListNode<TElement>> action)
        {
            var current = node;
            while (current != null)
            {
                var previous = current.Previous;

                action(current);

                current = previous;
            }
        }
    }
}