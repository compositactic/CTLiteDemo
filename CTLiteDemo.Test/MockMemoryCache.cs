// CTLiteDemo - Made in the USA - Indianapolis, IN  - Copyright (c) 2020 Matt J. Crouch

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

        public CacheItemPriority Priority { get; set; }
        public long? Size { get; set; }

        public void Dispose()
        {
        }
    }
}