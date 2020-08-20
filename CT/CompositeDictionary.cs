using CTLite.Properties;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace CTLite
{
    [Serializable]
    public class CompositeDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged where TValue : Composite
    {
        internal readonly ConcurrentDictionary<TKey, TValue> dictionary;

        public CompositeDictionary()
        {
            _synchronizationContext = AsyncOperationManager.SynchronizationContext;
            dictionary = new ConcurrentDictionary<TKey, TValue>();
            _removedIds = new ConcurrentBag<object>();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ConcurrentBag<object> _removedIds;
        public IEnumerable<object> RemovedIds => _removedIds;

        [NonSerialized]
        private readonly SynchronizationContext _synchronizationContext;
        private void RaiseEvents()
        {
            _synchronizationContext.Post(s =>
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ICollection<KeyValuePair<TKey, TValue>>.Count)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IDictionary<TKey, TValue>.Keys)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IDictionary<TKey, TValue>.Values)));
            }, null);
        }

        public void Add(TKey key, TValue value)
        {
            var result = dictionary.TryAdd(key, value);

            if (result)
                RaiseEvents();
        }

        public void AddRange(IEnumerable<TValue> composites)
        {
            KeyPropertyAttribute keyPropertyAttribute;
            PropertyInfo keyProperty;

            var compositeType = typeof(TValue);

            if ((keyPropertyAttribute = compositeType.GetCustomAttribute<KeyPropertyAttribute>()) == null)
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.MustHaveKeyPropertyAttribute, typeof(TValue).Name));

            if ((keyProperty = compositeType.GetProperty(keyPropertyAttribute.PropertyName)) == null)
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidPropertyName, keyPropertyAttribute.PropertyName));

            foreach (var composite in composites)
            {
                var keyValue = (TKey)keyProperty.GetValue(composite);
                Add(keyValue, composite);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return dictionary.Keys; }
        }

        public bool Remove(TKey key, bool setCompositeStateDeleted)
        {
            if (!setCompositeStateDeleted)
                return Remove(key);

            var valueToRemove = dictionary[key];

            var compositeModelAttribute = valueToRemove
                                        .GetType()
                                        .GetCustomAttribute<CompositeModelAttribute>();

            if (compositeModelAttribute == null)
                throw new MissingMemberException();

            var valueToRemoveModel = valueToRemove.GetType().GetField(compositeModelAttribute.ModelFieldName).GetValue(valueToRemove);
            var valueToRemoveId = valueToRemove.GetType().GetProperty(valueToRemoveModel.GetType().GetCustomAttribute<KeyPropertyAttribute>().PropertyName).GetValue(valueToRemoveModel);

            if (!_removedIds.Contains(valueToRemoveId))
                _removedIds.Add(valueToRemoveId);

            return Remove(key);
        }

        public void ClearRemovedIds()
        {
            _removedIds.Clear();
        }

        public bool Remove(TKey key)
        {
            var result = dictionary.TryRemove(key, out _);

            if (result)
                RaiseEvents();

            return result;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values
        {
            get { return dictionary.Values; }
        }

        public int Count
        {
            get { return dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).IsReadOnly; }
        }

        public TValue this[TKey key]
        {
            get { return dictionary[key]; }
            set
            {
                dictionary[key] = value;
                RaiseEvents();
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> newItem)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Add(newItem);
            RaiseEvents();
        }

        public void Clear()
        {
            dictionary.Clear();
            RaiseEvents();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var result = dictionary.TryRemove(item.Key, out _);

            if (result)
                RaiseEvents();

            return result;
        }
    }
}
