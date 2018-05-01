using Cottontail.Structure;
using System;

namespace Cottontail.Cache
{
    /// <summary>
    /// A cache key for live entities
    /// </summary>
    public class BackingDataCacheKey : ICacheKey, IEquatable<BackingDataCacheKey>
    {
        public CacheType CacheType
        {
            get { return CacheType.Stored; }
        }

        /// <summary>
        /// System type of the object being cached
        /// </summary>
        public Type ValueType { get; set; }

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
            ValueType = objectType;
            BirthMark = marker;
        }

        /// <summary>
        /// Hash key used by the cache system
        /// </summary>
        /// <returns>the key's hash</returns>
        public string KeyHash()
        {
            var typeName = ValueType.Name;

            //Normalize interfaces versus classnames
            if (ValueType.IsInterface)
                typeName = typeName.Substring(1);

            return string.Format("{0}_{1}_{2}", CacheType.ToString(), typeName, BirthMark.ToString());
        }

        #region Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (obj != null)
            {
                try
                {
                    if (obj.GetType() != GetType())
                        return -1;

                    var other = (ICacheKey)obj;

                    if (other.KeyHash().Equals(KeyHash()))
                        return 1;

                    return 0;
                }
                catch 
                {
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(ICacheKey other)
        {
            if (other != default(ICacheKey))
            {
                try
                {
                    return other.GetType() == GetType() 
                        && other.KeyHash().Equals(KeyHash());
                }
                catch 
                {
                }
            }

            return false;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(BackingDataCacheKey other)
        {
            if (other != default(BackingDataCacheKey))
            {
                try
                {
                    return other.GetType() == GetType() 
                        && other.KeyHash().Equals(KeyHash());
                }
                catch
                {
                }
            }

            return false;
        }
        #endregion
    }
}
