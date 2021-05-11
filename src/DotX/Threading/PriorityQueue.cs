using System;
using System.Collections.Generic;
using System.Linq;

namespace DotX.Threading
{
    internal class PriorityQueue<TElem, TPrio>
        where TPrio : IComparable
    {
        private readonly object _locker = new object();

        //TODO: use collection for multithreading.
        private SortedDictionary<TPrio, Queue<TElem>> _objectStore =
            new SortedDictionary<TPrio, Queue<TElem>>();

        public void Enqueue(TElem elemToAdd, TPrio prio)
        {
            lock (_locker)
            {
                if (!_objectStore.TryGetValue(prio, out var queue))
                {
                    queue = new Queue<TElem>();
                    _objectStore.Add(prio, queue);
                }

                queue.Enqueue(elemToAdd);
            }
        }

        public TElem Dequeue()
        {
            lock (_locker)
            {
                if (_objectStore.Count == 0)
                    throw new InvalidOperationException();

                var queue = _objectStore.Last();
                var elem = queue.Value.Dequeue();

                if (queue.Value.Count == 0)
                    _objectStore.Remove(queue.Key);

                return elem;
            }
        }

        public bool TryDequeue(out TElem value)
        {
            lock (_locker)
            {
                value = default;

                if (_objectStore.Count == 0)
                    return false;

                var queue = _objectStore.Last();

                var returnValue = queue.Value.TryDequeue(out value);

                if (queue.Value.Count == 0)
                    _objectStore.Remove(queue.Key);

                return returnValue;
            }
        }

        public TElem Peek()
        {
            lock (_locker)
            {
                if (_objectStore.Count == 0)
                    throw new InvalidOperationException();

                var queue = _objectStore.First();
                return queue.Value.Peek();
            }
        }
    }
}