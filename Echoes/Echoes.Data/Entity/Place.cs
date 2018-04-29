﻿using Cottontail.Cache;
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
        public ICacheContainer<IPersona> PersonaInventory { get; set; }

        [JsonIgnore]
        public ICacheContainer<IThing> ThingInventory { get; set; }

        [JsonConstructor]
        public Place() : base()
        {
        }

        public Place(StoredData storedData, StoredDataCache storedDataCache) : base(storedData, storedDataCache) 
        {
            PersonaInventory = new CacheContainer<IPersona>(storedDataCache);
            ThingInventory = new CacheContainer<IThing>(storedDataCache);
        }

        public override void SetAccessors(StoredData storedData, StoredDataCache storedDataCache)
        {
            PersonaInventory = new CacheContainer<IPersona>(storedDataCache);
            ThingInventory = new CacheContainer<IThing>(storedDataCache);

            base.SetAccessors(storedData, storedDataCache);
        }

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <returns>the contained entities</returns>
        public IEnumerable<IPersona> GetPersonas()
        {
            return PersonaInventory.Contents();
        }

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <returns>the contained entities</returns>
        public IEnumerable<IThing> GetThings()
        {
            return ThingInventory.Contents();
        }

        /// <summary>
        /// Method by which this entity has output (from commands and events) "shown" to it
        /// </summary>
        public override bool WriteTo(string input, IEntity originator)
        { 
            foreach(var entity in GetThings())
                entity.WriteTo(input, originator);

            foreach (var entity in GetPersonas())
                entity.WriteTo(input, originator);

            return TriggerAIAction(input);
        }

        /// <summary>
        /// Move an entity into a named container in this
        /// </summary>
        /// <param name="thing">the entity to add</param>
        /// <returns>errors</returns>
        public bool MoveInto(IPersona thing)
        {
            if (PersonaInventory.Contains(thing))
                return false;

            PersonaInventory.Add(thing);
            thing.Position = this;
            UpsertToLiveWorldCache();

            return true;
        }

        /// <summary>
        /// Move an entity into a named container in this
        /// </summary>
        /// <param name="thing">the entity to add</param>
        /// <returns>errors</returns>
        public bool MoveInto(IThing thing)
        {
            if (ThingInventory.Contains(thing))
                return false;

            ThingInventory.Add(thing);
            thing.Position = this;
            UpsertToLiveWorldCache();

            return true;
        }

        /// <summary>
        /// Move an entity out of this' named container
        /// </summary>
        /// <param name="thing">the entity</param>
        /// <param name="containerName">the name of the container</param>
        /// <returns>errors</returns>
        public bool MoveFrom(IPersona thing)
        {
            if (!PersonaInventory.Contains(thing))
                return false;

            PersonaInventory.Remove(thing);
            thing.Position = null;
            UpsertToLiveWorldCache();

            return true;
        }

        public bool MoveFrom(IThing thing)
        {
            if (!ThingInventory.Contains(thing))
                return false;

            ThingInventory.Remove(thing);
            thing.Position = null;
            UpsertToLiveWorldCache();

            return true;
        }
        #endregion

        public override IEnumerable<string> RenderToLook()
        {
            var output = new List<string>();

            output.AddRange(RenderSelf());

            foreach (var thing in ThingInventory)
                output.AddRange(thing.RenderToLocation());

            foreach (var thing in PersonaInventory)
                output.AddRange(thing.RenderToLocation());

            return output;
        }

        private IEnumerable<string> RenderSelf()
        {
            var sb = new List<string>
            {
                string.Format("<H3>{0}</H3>", Name),
                string.Empty.PadLeft(Name.Length, '-')
            };

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
        /// Update this entry to the live world cache
        /// </summary>
        public override void UpsertToLiveWorldCache()
        {
            DataCache.Add(this as IPlace);
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
