using Cottontail.FileSystem.Logging;
using Cottontail.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace Cottontail.Cache
{
    public abstract class CacheAccessor
    {
        /* 
         * The general idea here is that we are literally caching everything possible from app start.
         * 
         * We'll need to cache collections of references to things and the things themselves.
         * Caching collections of things will result in flipping the cache constantly
         * 
         * The administrative website will edit the Lookup Data in the database which wont get refreshed
         * until someone tells it to (or the entire thing reboots)
         * 
         * IData data is ALWAYS cached and saved to a different place because it is live in-game data and even if
         * we add, say, one damage to the Combat Knife item in the db it doesn't mean all Combat Knife objects in game
         * get retroactively updated. There will be superadmin level website commands to do this and in-game commands for admins.
         * 
         */

        /// <summary>
        /// The place everything gets stored
        /// </summary>
        internal IMemoryCache _globalCache;

        internal Dictionary<Type, HashSet<ICacheKey>> _keysByType;

        internal FileLogger Logger { get; }

        /// <summary>
        /// The cache type (affects the "ids")
        /// </summary>
        internal CacheType _type;

        /// <summary>
        /// Create a new CacheAccessor with its type
        /// </summary>
        /// <param name="type">The type of item we're caching</param>
        internal CacheAccessor(CacheType type, IMemoryCache memoryCache, FileLogger logger)
        {
            _keysByType = new Dictionary<Type, HashSet<ICacheKey>>();

            _globalCache = memoryCache;

            _type = type;

            Logger = logger;
        }

        /// <summary>
        /// fills a list of entities from the cache of a single type that match the birthmarks sent in
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <returns>a list of the entities from the cache</returns>
        public virtual IEnumerable<T> GetMany<T>(IEnumerable<ICacheKey> ids) where T : IData
        {
            try
            {
                var returnList = new List<T>();

                foreach (var id in ids)
                    returnList.Add(Get<T>(id));

                return returnList;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            return Enumerable.Empty<T>();
        }
        
        /// <summary>
        /// Get all entities of a type from the cache
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <returns>a list of the entities from the cache</returns>
        public virtual IEnumerable<T> GetAll<T>()
        {
            try
            {
                var returnList = new List<T>();

                foreach (var key in GetKeys(typeof(T)))
                    returnList.Add(Get<T>(key));

                return returnList;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Gets one entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public virtual T Get<T>(ICacheKey key)
        {
            return Get<T>(key.KeyHash());
        }

        /// <summary>
        /// Gets one non-entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public virtual T Get<T>(string key)
        {
            try
            {
                return _globalCache.Get<T>(key);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            return default(T);
        }

        /// <summary>
        /// Adds an object to the cache
        /// </summary>
        /// <param name="objectToCache">the object to cache</param>
        /// <param name="cacheKey">the key to cache it under</param>
        public virtual void Add<T>(object objectToCache, ICacheKey cacheKey)
        {
            if (Exists(cacheKey))
                Remove<T>(cacheKey);

            AddKey(cacheKey.ValueType, cacheKey);

            _globalCache.Set(cacheKey.KeyHash(), objectToCache);
        }

        /// <summary>
        /// Removes an entity from the cache by its key
        /// </summary>
        /// <param name="key">the key of the entity to remove</param>
        public virtual void Remove<T>(ICacheKey key)
        {
            _globalCache.Remove(key.KeyHash());

            RemoveKey(key.ValueType, key);
        }

        /// <summary>
        /// Checks if an entity is in the cache
        /// </summary>
        /// <param name="key">the key of the entity</param>
        /// <returns>if it is in the cache of not</returns>
        public virtual bool Exists(ICacheKey key)
        {
            return Exists(key.KeyHash());
        }

        /// <summary>
        /// Checks if an non-entity is in the cache
        /// </summary>
        /// <param name="key">the key of the entity</param>
        /// <returns>if it is in the cache of not</returns>
        public bool Exists(string key)
        {
            return _globalCache.Get(key) != null;
        }

        internal virtual void AddKey(Type type, ICacheKey key)
        {
            var currentList = new HashSet<ICacheKey>();

            if (_keysByType.ContainsKey(type))
            {
                currentList = _keysByType[type];

                //Already got it and this is just a keystore not the actual object
                if(currentList.Any(ky => ky.Equals(key)))
                    return;

                _keysByType.Remove(type);
            }

            currentList.Add(key);
            _keysByType.Add(type, currentList);
        }

        internal virtual void RemoveKey(Type type, ICacheKey key)
        {
            //Doesn't exist just leave
            if (!_keysByType.ContainsKey(type))
                return;

            var currentList = _keysByType[type];

            currentList.Remove(key);
        }

        internal virtual HashSet<ICacheKey> GetKeys(Type type)
        {
            if (!_keysByType.ContainsKey(type))
                return new HashSet<ICacheKey>();

            return _keysByType[type];
        }
    }
}
