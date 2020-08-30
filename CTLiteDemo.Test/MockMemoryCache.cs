using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace CTLiteDemo.Test
{
    internal class MockMemoryCache : IMemoryCache
    {

        public ICacheEntry CreateEntry(object key)
        {
            return new MockCacheEntry { Key = key };
        }

        public void Dispose()
        {
        }

        public void Remove(object key)
        {
            System.Runtime.Caching.MemoryCache.Default.Remove(key.ToString());
        }

        public bool TryGetValue(object key, out object value)
        {
            var keyString = key.ToString();
            if(System.Runtime.Caching.MemoryCache.Default.Contains(keyString))
            {
                value = System.Runtime.Caching.MemoryCache.Default.Get(keyString);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }

    internal class MockCacheEntry : ICacheEntry
    {

        public object Key { get; set; }

        private object _value;
        public object Value
        { 
            get { return _value; }
            set
            {
                _value = value;
                System.Runtime.Caching.MemoryCache.Default.Set(Key.ToString(), value, DateTimeOffset.MaxValue);
            }
        }

        public DateTimeOffset? AbsoluteExpiration { get; set; } = DateTimeOffset.MaxValue;

        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; } = TimeSpan.MaxValue;

        public TimeSpan? SlidingExpiration { get; set; } = TimeSpan.MaxValue;

        public IList<IChangeToken> ExpirationTokens { get; } = new List<IChangeToken>();

        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = new List<PostEvictionCallbackRegistration>();

        public Microsoft.Extensions.Caching.Memory.CacheItemPriority Priority { get; set; }
        public long? Size { get; set; }

        public void Dispose()
        {
        }
    }
}