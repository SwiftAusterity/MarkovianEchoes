using Cottontail.Cache;
using Cottontail.FileSystem;
using Cottontail.FileSystem.Logging;
using Echoes.Data.System;
using Echoes.DataStructure.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Echoes.Data.Entity
{
    [Serializable]
    public class Thing : EntityPartial, IThing
    {
        [JsonConstructor]
        public Thing() : base() { }

        public Thing(StoredData storedData, StoredDataCache storedDataCache) : base(storedData, storedDataCache) { }

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            //Things dont "need" to be spawned anywhere
            UpsertToLiveWorldCache();
        }

        public override IEnumerable<string> RenderToLocation()
        {
            var sb = new List<string>();

            sb.Add(string.Format("{0} is here", Name));

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
                LoggingUtility.LogError(BaseDirectory, ex);
                return false;
            }

            return true;
        }
    }
}
