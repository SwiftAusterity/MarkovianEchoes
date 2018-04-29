using Cottontail.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cottontail.Cache
{
    /// <summary>
    /// Object that handles any and all "this contains entities" for the system
    /// </summary>
    /// <typeparam name="T">the type of entities it can contain</typeparam>
    [Serializable]
    public class CacheContainer<T> : ICacheContainer<T> where T : IData
    {
        /// <summary>
        /// What this actually contains, yeah it's a hashtable of hashtables but whatever
        /// </summary>
        private List<long> IDKeys;

        private StoredDataCache dataCache;

        int ICollection<T>.Count => IDKeys.Count;

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get
            {
                return Contents().ElementAt(index);
            }
            set
            {
                IDKeys.RemoveAt(index);
                IDKeys.Insert(index, value.ID);
            }
        }

        public CacheContainer(StoredDataCache storedDataCache)
        {
            dataCache = storedDataCache;
            IDKeys = new List<long>();
        }

        /// <summary>
        /// Restful list of entities contained (it needs to never store its own objects, only cache references)
        /// </summary>
        public IEnumerable<T> Contents()
        {
            if (this.Count() > 0)
                return dataCache.GetMany<T>(IDKeys);

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Does this contain the specified item
        /// </summary>
        /// <param name="item">the item in question</param>
        /// <returns>yes it contains it or no it does not</returns>
        public bool Contains(T item)
        {
            return IDKeys.Any(k => k == item.ID);
        }

        /// <summary>
        /// Add an item to this
        /// </summary>
        /// <param name="item">the item to add</param>
        /// <returns>success status</returns>
        public bool Add(T item)
        {
            IDKeys.Add(item.ID);

            return true;
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void Insert(int index, T item)
        {
            IDKeys.Insert(index, item.ID);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Contents().ToList().CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Remove an item from this
        /// </summary>
        /// <param name="item">the item to remove</param>
        /// <returns>success status</returns>
        public bool Remove(T item)
        {
            return Remove(item.ID);
        }

        /// <summary>
        /// Remove an item from this
        /// </summary>
        /// <param name="birthMark">the item's birthmark to remove</param>
        /// <returns>success status</returns>
        public bool Remove(long id)
        {
            return IDKeys.Remove(id);
        }

        public void RemoveAt(int index)
        {
            IDKeys.RemoveAt(index);
        }

        public void Clear()
        {
            IDKeys.Clear();
        }

        public int IndexOf(T item)
        {
            return IDKeys.IndexOf(item.ID);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Contents().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Contents().GetEnumerator();
        }
    }
}
