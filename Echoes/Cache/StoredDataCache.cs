using Cottontail.Structure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using Utility;

namespace Cottontail.Cache
{
    /// <summary>
    /// Storage and access for live entities in game (including players)
    /// </summary>
    public class StoredDataCache : CacheAccessor, IDisposable
    {
        public StoredDataCache(IHostingEnvironment hostingEnvironment, IMemoryCache memoryCache) : base(CacheType.Stored, memoryCache, hostingEnvironment.ContentRootPath)
        {
        }

        /// <summary>
        /// Adds a single entity into the cache
        /// </summary>
        /// <param name="objectToCache">the entity to cache</param>
        public void Add<T>(T objectToCache) where T : IData
        {
            var entityToCache = (IData)objectToCache;
            var cacheKey = new BackingDataCacheKey(objectToCache.GetType(), entityToCache.ID);

            Add<T>(objectToCache, cacheKey);
        }

        public IEnumerable<IData> GetAll(Type objType)
        {
            var returnList = new List<IData>();
            var keys = _keysByType.Where(key => key.Key == objType || TypeUtility.GetAllImplimentingedTypes(key.Key).Contains(objType)).SelectMany(key => key.Value);

            foreach(var key in keys)
                returnList.Add((IData)_globalCache.Get(key.KeyHash()));            

            return returnList;
        }

        /// <summary>
        /// fills a list of entities from the cache of a single type that match the birthmarks sent in
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <param name="birthmarks">the birthmarks to retrieve</param>
        /// <returns>a list of the entities from the cache</returns>
        public IEnumerable<T> GetMany<T>(IEnumerable<long> ids) where T : IData
        {
            var idKeys = ids.Select(id => new BackingDataCacheKey(typeof(T), id));

            return GetMany<T>(idKeys);
        }
        
        /// <summary>
        /// Gets one non-entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public T GetByName<T>(string name) where T : IData
        {
            var cacheItems = GetAll(typeof(T));

            return (T)cacheItems.FirstOrDefault(ci => ci.Name.Contains(name));
        }
        
        /// <summary>
        /// Gets one entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public T Get<T>(BackingDataCacheKey key) where T : IData
        {
            return Get<T>(key.KeyHash());
        }

        /// <summary>
        /// Gets one entity from the cache by its ID, only works for Singleton spawners with data templates(IEntities)
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="id">the id</param>
        /// <returns>the entity requested</returns>
        public T Get<T>(long id) where T : IData
        {
            var key = new BackingDataCacheKey(typeof(T), id);

            return Get<T>(key);
        }

        /// <summary>
        /// Removes an entity from the cache by its key
        /// </summary>
        /// <param name="key">the key of the entity to remove</param>
        public void Remove<T>(BackingDataCacheKey key)
        {
            Remove<T>(key);
        }

        /// <summary>
        /// Checks if an entity is in the cache
        /// </summary>
        /// <param name="key">the key of the entity</param>
        /// <returns>if it is in the cache of not</returns>
        public bool Exists(BackingDataCacheKey key)
        {
            return Exists(key);
        }

        public void Dispose()
        {
        }
    }
}
