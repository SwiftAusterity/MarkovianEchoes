using System.Collections.Generic;

namespace Cottontail.Structure
{
    /// <summary>
    /// Object that handles any and all "this contains cached objects" for the system
    /// </summary>
    /// <typeparam name="T">the type of entities it can contain</typeparam>
    public interface ICacheContainer<T> : IList<T> where T : IData
    {
        /// <summary>
        /// List of data contained (it needs to never store its own objects, only cache references)
        /// </summary>
        IEnumerable<T> Contents();

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="ID">the data's ID to remove</param>
        /// <returns>success status</returns>
        bool Remove(long ID);
    }
}
