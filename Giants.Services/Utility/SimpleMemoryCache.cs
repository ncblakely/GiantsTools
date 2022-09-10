using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Giants.Services.Utility
{
    /// <summary>
    /// Wrapper around <see cref="MemoryCache"/> that caches all items of the specified type.
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    public class SimpleMemoryCache<TRecord> : ISimpleMemoryCache<TRecord>, IDisposable
    {
        private readonly TimeSpan? expirationPeriod;
        private IMemoryCache memoryCache;
        private readonly object cacheKey;
        private readonly Func<ICacheEntry, Task<IEnumerable<TRecord>>> getAllItems;
        private CancellationTokenSource resetCacheToken;
        private bool disposedValue;

        public SimpleMemoryCache(
            TimeSpan? expirationPeriod,
            IMemoryCache memoryCache,
            object cacheKey,
            Func<ICacheEntry, Task<IEnumerable<TRecord>>> getAllItems)
        {
            this.expirationPeriod = expirationPeriod;
            this.memoryCache = memoryCache;
            this.cacheKey = cacheKey;
            this.getAllItems = getAllItems;
            this.resetCacheToken = new CancellationTokenSource();
        }

        public async Task<IEnumerable<TRecord>> GetItems()
        {
            IEnumerable<TRecord> items = await this.memoryCache.GetOrCreateAsync(cacheKey, this.PopulateCache);
            return items;
        }

        public void Invalidate()
        {
            this.resetCacheToken.Cancel();
            this.resetCacheToken.Dispose();
            this.resetCacheToken = new CancellationTokenSource();
        }

        private async Task<IEnumerable<TRecord>> PopulateCache(ICacheEntry cacheEntry)
        {
            if (this.expirationPeriod.HasValue)
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = this.expirationPeriod;
            }

            cacheEntry.AddExpirationToken(new CancellationChangeToken(this.resetCacheToken.Token));

            return await this.getAllItems(cacheEntry);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.memoryCache?.Dispose();
                    this.resetCacheToken?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
