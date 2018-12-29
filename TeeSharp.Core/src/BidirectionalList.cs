using System;
using System.Collections;
using System.Collections.Generic;

namespace TeeSharp.Core
{
    public class BidirectionalList<T> : IEnumerable<T>
    {
        public class Node
        {
            public Node Prev { get; set; }
            public Node Next { get; set; }
            public T Value { get; set; }
        }

        public Node First { get; private set; }
        public Node Last { get; private set; }
        public int Count { get; private set; }

        private BidirectionalList()
        {
            Last = Last = null;
            Count = 0;
        }

        public static BidirectionalList<T> New()
        {
            return new BidirectionalList<T>();
        }

        public Node Add(T item)
        {
            var node = new Node()
            {
                Value = item,
                Next = null,
                Prev = Last
            };

            if (First == null)
                First = node;

            if (Last != null)
                Last.Next = node;

            Last = node;
            Count++;

            return node;
        }

        public void RemoveFast(Node node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (node.Prev != null)
                node.Prev.Next = node.Next;
            else
                First = node.Next; // remove head

            if (node.Next != null)
                node.Next.Prev = node.Prev;
            else
                Last = node.Prev; // remove last

            node.Next = null;
            node.Prev = null;

            Count--;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var current = First;
            while (current != null)
            {
                yield return current.Value;
                current = current.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}