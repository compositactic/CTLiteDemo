using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace CTLiteDemo.Test
{
    internal class MockMemoryCache : IMemoryCache
    {
        private readonly Dictionary<object, object> _cache = new Dictionary<object, object>();
        public ICacheEntry CreateEntry(object key)
        {
            return new MockCacheEntry { Key = key };
        }

        public void Dispose()
        {
        }

        public void Remove(object key)
        {
            _cache.Remove(key);
        }

        public bool TryGetValue(object key, out object value)
        {
            if(_cache.ContainsKey(key))
            {
                value = _cache[key];
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

        public object Value { get; set; }

        public DateTimeOffset? AbsoluteExpiration { get; set; } = DateTimeOffset.MaxValue;

        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; } = TimeSpan.MaxValue;

        public TimeSpan? SlidingExpiration { get; set; } = TimeSpan.MaxValue;

        public IList<IChangeToken> ExpirationTokens { get; } = new List<IChangeToken>();

        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = new List<PostEvictionCallbackRegistration>();

        public CacheItemPriority Priority { get; set; }
        public long? Size { get; set; }

        public void Dispose()
        {
        }
    }
}