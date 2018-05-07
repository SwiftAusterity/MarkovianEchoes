using Cottontail.Cache;
using Cottontail.FileSystem;
using Cottontail.FileSystem.Logging;
using Cottontail.Structure;
using Echoes.Data.Contextual;
using Echoes.Data.System;
using Echoes.DataStructure.Contextual;
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

        public Place(StoredDataFileAccessor storedData, StoredDataCache storedDataCache, FileLogger logger) : base(storedData, storedDataCache, logger) 
        {
            PersonaInventory = new CacheContainer<IPersona>(storedDataCache);
            ThingInventory = new CacheContainer<IThing>(storedDataCache);
        }

        public override void SetAccessors(StoredDataFileAccessor storedData, StoredDataCache storedDataCache, FileLogger logger)
        {
            PersonaInventory = new CacheContainer<IPersona>(storedDataCache);
            ThingInventory = new CacheContainer<IThing>(storedDataCache);

            base.SetAccessors(storedData, storedDataCache, logger);
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
        public override IEnumerable<IContext> WriteTo(string input, IPersona originator, bool acting)
        { 
            foreach(var entity in GetThings())
                entity.WriteTo(input, originator, acting);

            foreach (var entity in GetPersonas())
                entity.WriteTo(input, originator, acting);

            return base.WriteTo(input, originator, acting);
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
            thing.Save();
            Save();

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
            thing.Save();
            Save();

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
            return RenderSelf();
        }

        private IEnumerable<string> RenderSelf()
        {
            var decorators = FullContext.Where(adj => adj.GetType() == typeof(Descriptor) && ((Descriptor)adj).Applied).Select(desc => desc.Name);

            var sb = new List<string>();

            if(decorators.Count() > 0)
                sb.Add(string.Format("It is quite {0} here.", String.Join(",", decorators)));

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
                Logger.LogError(ex);
                return false;
            }

            return true;
        }
    }
}
