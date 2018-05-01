using System;

namespace Cottontail.Structure
{
    /// <summary>
    /// A cache key
    /// </summary>
    public interface ICacheKey : IComparable, IEquatable<ICacheKey>
    {
        /// <summary>
        /// The type of cache this is for
        /// </summary>
        CacheType CacheType { get; }

        /// <summary>
        /// The type of the value that the key is for
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        /// Hash key used by the cache system
        /// </summary>
        /// <returns>the key's hash</returns>
        string KeyHash();
    }
}
