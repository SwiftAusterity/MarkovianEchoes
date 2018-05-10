using Cottontail.Cache;
using Cottontail.FileSystem;
using Cottontail.FileSystem.Logging;
using Echoes.Data.Contextual;
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
    public class Thing : EntityPartial, IThing
    {
        [JsonConstructor]
        public Thing() : base() { }

        public Thing(StoredDataFileAccessor storedData, StoredDataCache storedDataCache, FileLogger logger) : base(storedData, storedDataCache, logger) { }

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            //Things dont "need" to be spawned anywhere
            UpsertToLiveWorldCache();
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IContains spawnTo)
        {
            spawnTo.MoveInto(this);
            UpsertToLiveWorldCache();
        }

        /// <summary>
        /// Update this entry to the live world cache
        /// </summary>
        public override void UpsertToLiveWorldCache()
        {
            DataCache.Add(this as IThing);
        }

        /// <summary>
        /// Move this inside of something
        /// </summary>
        /// <param name="container">The container to move into</param>
        /// <returns>was this thing moved?</returns>
        public override bool TryMoveInto(IContains container)
        {
            return container.MoveInto(this);
        }

        public override IEnumerable<string> RenderToLocation()
        {
            var decorators = FullContext.Where(adj => adj.GetType() == typeof(Descriptor) && ((Descriptor)adj).Applied).Select(desc => desc.Name);

            var sb = new List<string>
            {
                string.Format("A {1} {0}<sup><a href='#' class='entityInfo' entityType='thing' entityName='{0}'>?</a></sup> is here.", Name, String.Join(",", decorators))
            };

            return sb;
        }

        public override IEnumerable<string> RenderToLook()
        {
            var sb = new List<string>();

            sb.Add(string.Format("<s>{0}</s>", Name));

            return sb;
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
                DataCache.Remove<IThing>(new BackingDataCacheKey(this.GetType(), this.ID));

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
