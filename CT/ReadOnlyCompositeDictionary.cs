using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CTLite
{
    [Serializable]
    public class ReadOnlyCompositeDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged where TValue : Composite
    {
        public ReadOnlyCompositeDictionary(CompositeDictionary<TKey, TValue> compositeDictionary)
        {
            _dictionary = compositeDictionary ?? throw new ArgumentNullException(nameof(compositeDictionary));
            _dictionary.CollectionChanged += (sender, e) => { CollectionChanged?.Invoke(this, e); };
            _dictionary.PropertyChanged += (sender, e) => { PropertyChanged?.Invoke(this, e); };
        }

        public IEnumerable<object> RemovedIds
        {
            get { return _dictionary.RemovedIds; }
        }

        private readonly CompositeDictionary<TKey, TValue> _dictionary;
        protected IDictionary<TKey, TValue> Dictionary
        {
            get { return _dictionary; }
        }

        public TValue this[TKey key]
        {
            get { return TryGetValue(key, out TValue value) ? value : default; }
        }

        public int Count
        {
            get { return Dictionary.Count; }
        }

        public IEnumerable<TKey> Keys
        {
            get { return Dictionary.Keys; }
        }

        public IEnumerable<TValue> Values
        {
            get { return Dictionary.Values; }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public bool ContainsKey(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Dictionary).GetEnumerator();
        }
    }
}
