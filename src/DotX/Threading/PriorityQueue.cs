using System;
using System.Collections.Generic;
using System.Linq;

namespace DotX.Threading
{
    internal class PriorityQueue<TElem, TPrio>
        where TPrio : IComparable
    {
        private SortedDictionary<TPrio, Queue<TElem>> _objectStore =
            new SortedDictionary<TPrio, Queue<TElem>>();

        public void Enqueue(TElem elemToAdd, TPrio prio)
        {
            if(!_objectStore.TryGetValue(prio, out var queue))
            {
                queue = new Queue<TElem>();
                _objectStore.Add(prio, queue);
            }

            queue.Enqueue(elemToAdd);
        }

        public TElem Dequeue()
        {
            if(_objectStore.Count == 0)
                throw new InvalidOperationException();

            var queue = _objectStore.First();
            var elem = queue.Value.Dequeue();

            if(queue.Value.Count == 0)
                _objectStore.Remove(queue.Key);

            return elem;
        }

        public bool TryDequeue(out TElem value)
        {
            if(_objectStore.Count == 0)
                throw new InvalidOperationException();

            var queue = _objectStore.First();
            
            return queue.Value.TryDequeue(out value);
        }

        public TElem Peek()
        {
            if(_objectStore.Count == 0)
                throw new InvalidOperationException();

            var queue = _objectStore.First();
            return queue.Value.Peek();
        }
    }
}