using Cottontail.Cache;
using Echoes.DataStructure.System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Echoes.Data.System
{
    /// <summary>
    /// Object that handles any and all "this contains entities" for the system
    /// </summary>
    /// <typeparam name="T">the type of entities it can contain</typeparam>
    public class EntityContainer : IEntityContainer
    {
        private const string genericCollectionLabel = "*VoidContainer*";
        /// <summary>
        /// What this actually contains, yeah it's a hashtable of hashtables but whatever
        /// </summary>
        private Dictionary<string, HashSet<long>> IDKeys;

        /// <summary>
        /// New up an empty container
        /// </summary>
        [JsonConstructor]
        public EntityContainer()
        {
            IDKeys = new Dictionary<string, HashSet<long>>();

            IDKeys.Add(genericCollectionLabel, new HashSet<long>());
        }

        /// <summary>
        /// Restful list of entities contained (it needs to never store its own objects, only cache references)
        /// </summary>
        public IEnumerable<IEntity> EntitiesContained()
        {
            if (Count() > 0)
                return StoredDataCache.GetMany<IEntity>(IDKeys.Values.SelectMany(hs => hs));

            return Enumerable.Empty<IEntity>();
        }

        /// <summary>
        /// Add an entity to this
        /// </summary>
        /// <param name="entity">the entity to add</param>
        /// <returns>success status</returns>
        public bool Add(IEntity entity)
        {
            return IDKeys[genericCollectionLabel].Add(entity.ID);
        }

        /// <summary>
        /// Does this contain the specified entity
        /// </summary>
        /// <param name="entity">the entity in question</param>
        /// <returns>yes it contains it or no it does not</returns>
        public bool Contains(IEntity entity)
        {
            return IDKeys.Values.Any(hs => hs.Contains(entity.ID));
        }

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="entity">the entity to remove</param>
        /// <returns>success status</returns>
        public bool Remove(IEntity entity)
        {
            return IDKeys[genericCollectionLabel].Remove(entity.ID);
        }

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="birthMark">the entity's birthmark to remove</param>
        /// <returns>success status</returns>
        public bool Remove(long id)
        {
            return IDKeys[genericCollectionLabel].Remove(id);
        }

        /// <summary>
        /// Count the entities in this
        /// </summary>
        /// <returns>the count</returns>
        public int Count()
        {
            return IDKeys.Values.Sum(hs => hs.Count);
        }   
    }
}
