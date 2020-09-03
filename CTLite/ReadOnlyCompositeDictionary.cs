// CTLite - Made in the USA - Indianapolis, IN  - Copyright (c) 2020 Matt J. Crouch

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN 
// NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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
