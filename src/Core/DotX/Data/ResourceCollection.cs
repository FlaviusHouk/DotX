using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DotX.Data
{
    public class ResourceCollection : Observable<string>,
                                      IDictionary<string, object>
    {
        private readonly Dictionary<string, object> _storage = 
            new();

        public object this[string key] 
        { 
            get => _storage[key]; 
            set
            {
                _storage[key] = value;

                foreach(var observer in Observers)
                    observer.OnNext(key);
            }  
        }

        public ICollection<string> Keys => _storage.Keys;

        public ICollection<object> Values => _storage.Values;

        public int Count => _storage.Count;

        public bool IsReadOnly => false;

        public void Add(string key, object value)
        {
            _storage.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            ((ICollection<KeyValuePair<string, object>>)_storage).Add(item);
        }

        public void Clear()
        {
            _storage.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ((ICollection<KeyValuePair<string, object>>)_storage).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _storage.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, object>>)_storage).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _storage.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _storage.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return ((ICollection<KeyValuePair<string, object>>)_storage).Remove(item);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            return _storage.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _storage.GetEnumerator();
        }
    }
}