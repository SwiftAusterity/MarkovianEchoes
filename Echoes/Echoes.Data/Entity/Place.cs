using Cottontail.Cache;
using Cottontail.FileSystem;
using Cottontail.FileSystem.Logging;
using Cottontail.Structure;
using Echoes.Data.System;
using Echoes.DataStructure.Entity;
using Echoes.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Echoes.Data.Entity
{
    [Serializable]
    public class Place : EntityPartial, IPlace
    {
        #region Container
        [JsonIgnore]
        public ICacheContainer<IEntity> Inventory { get; set; }

        [JsonConstructor]
        public Place() : base() { }

        public Place(StoredData storedData, StoredDataCache storedDataCache) : base(storedData, storedDataCache) 
        {
            Inventory = new CacheContainer<IEntity>(storedDataCache);
        }

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        public IEnumerable<IEntity> GetContents()
        {
            return Inventory.Contents();
        }

        /// <summary>
        /// Method by which this entity has output (from commands and events) "shown" to it
        /// </summary>
        public override bool WriteTo(string input, IEntity originator)
        {
            //Hacky "Each" to write output to everything inside this place
            GetContents().Select(entity => entity.WriteTo(input, originator));

            return TriggerAIAction(input);
        }

        /// <summary>
        /// Move an entity into a named container in this
        /// </summary>
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <returns>errors</returns>
        public bool MoveInto(IEntity thing)
        {
            if (Inventory.Contains(thing))
                return false;

            Inventory.Add(thing);
            thing.Position = this;
            UpsertToLiveWorldCache();

            return true;
        }

        /// <summary>
        /// Move an entity out of this' named container
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <param name="containerName">the name of the container</param>
        /// <returns>errors</returns>
        public bool MoveFrom(IEntity thing)
        {
            if (!Inventory.Contains(thing))
                return false;

            Inventory.Remove(thing);
            thing.Position = null;
            UpsertToLiveWorldCache();

            return true;
        }
        #endregion

        public override IEnumerable<string> RenderToLook()
        {
            var output = new List<string>();

            output.AddRange(RenderSelf());

            foreach (var thing in Inventory)
                output.AddRange(thing.RenderToLocation());

            return output;
        }

        private IEnumerable<string> RenderSelf()
        {
            var sb = new List<string>();

            sb.Add(string.Format("<H3>{0}</H3>", Name));
            sb.Add(string.Empty.PadLeft(Name.Length, '-'));

            return sb;
        }

        public override IEnumerable<string> RenderToLocation()
        {
            //Return the look outpt just incase as this IS the location
            return RenderToLook();
        }

        public override void SpawnNewInWorld()
        {
            UpsertToLiveWorldCache();
        }

        public override void SpawnNewInWorld(IContains spawnTo)
        {
            //Places are containers, they don't spawn to anything

            UpsertToLiveWorldCache();
        }

        /// <summary>
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public override bool Remove()
        {
            try
            {
                //Remove from cache first
                DataCache.Remove<IPlace>(new BackingDataCacheKey(this.GetType(), this.ID));

                //Remove it from the file system.
                DataStore.ArchiveEntity(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(BaseDirectory, ex);
                return false;
            }

            return true;
        }
    }
}
