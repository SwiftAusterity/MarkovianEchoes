﻿using Cottontail.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cottontail.Cache
{
    /// <summary>
    /// Storage and access for live entities in game (including players)
    /// </summary>
    public class StoredDataCache : IDisposable
    {
        private CacheAccessor BackingCache;

        public StoredDataCache(string baseDirectory)
        {
            BackingCache = new CacheAccessor(CacheType.Stored, baseDirectory);
        }

        /// <summary>
        /// Adds a single entity into the cache
        /// </summary>
        /// <param name="objectToCache">the entity to cache</param>
        public void Add<T>(T objectToCache) where T : IData
        {
            var entityToCache = (IData)objectToCache;
            var cacheKey = new BackingDataCacheKey(objectToCache.GetType(), entityToCache.ID);

            BackingCache.Add<T>(objectToCache, cacheKey);
        }

        /// <summary>
        /// fills a list of entities from the cache of a single type that match the birthmarks sent in
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <param name="birthmarks">the birthmarks to retrieve</param>
        /// <returns>a list of the entities from the cache</returns>
        public IEnumerable<T> GetMany<T>(IEnumerable<long> ids) where T : IData
        {
            return BackingCache.GetMany<T>(ids);
        }

        /// <summary>
        /// Get all entities of a type from the cache
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <returns>a list of the entities from the cache</returns>
        public IEnumerable<T> GetAll<T>()
        {
            return BackingCache.GetAll<T>();
        }

        /// <summary>
        /// Only for the hotbackup procedure
        /// </summary>
        /// <returns>All entities in the entire system</returns>
        public IEnumerable<IData> GetAll()
        {
            return BackingCache.GetAll<IData>();
        }

        /// <summary>
        /// Gets one non-entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public T GetByName<T>(string name) where T : IData
        {
            var cacheItems = BackingCache.GetAll<T>();

            return cacheItems.FirstOrDefault<T>(ci => ci.Name.Contains(name));
        }

        /// <summary>
        /// Gets one non-entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public T Get<T>(string key)
        {
            return BackingCache.Get<T>(key);
        }

        /// <summary>
        /// Gets one entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public T Get<T>(BackingDataCacheKey key) where T : IData
        {
            return BackingCache.Get<T>(key);
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
            BackingCache.Remove<T>(key);
        }

        /// <summary>
        /// Checks if an entity is in the cache
        /// </summary>
        /// <param name="key">the key of the entity</param>
        /// <returns>if it is in the cache of not</returns>
        public bool Exists(BackingDataCacheKey key)
        {
            return BackingCache.Exists(key);
        }

        public void Dispose()
        {
        }
    }

    /// <summary>
    /// A cache key for live entities
    /// </summary>
    public class BackingDataCacheKey : ICacheKey
    {
        public CacheType CacheType
        {
            get { return CacheType.Stored; }
        }

        /// <summary>
        /// System type of the object being cached
        /// </summary>
        public Type ObjectType { get; set; }

        /// <summary>
        /// Unique signature for a live object
        /// </summary>
        public long BirthMark { get; set; }

        /// <summary>
        /// Generate a live key for a live object
        /// </summary>
        /// <param name="objectType">System type of the entity being cached</param>
        /// <param name="marker">Unique signature for a live entity</param>
        public BackingDataCacheKey(Type objectType, long marker)
        {
            ObjectType = objectType;
            BirthMark = marker;
        }

        /// <summary>
        /// Hash key used by the cache system
        /// </summary>
        /// <returns>the key's hash</returns>
        public string KeyHash()
        {
            var typeName = ObjectType.Name;

            //Normalize interfaces versus classnames
            if (ObjectType.IsInterface)
                typeName = typeName.Substring(1);

            return string.Format("{0}_{1}_{2}", CacheType.ToString(), typeName, BirthMark.ToString());
        }
    }
}
