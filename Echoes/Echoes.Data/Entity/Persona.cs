using Cottontail.Cache;
using Cottontail.FileSystem;
using Cottontail.FileSystem.Logging;
using Echoes.Data.Contextual;
using Echoes.Data.System;
using Echoes.DataStructure.Contextual;
using Echoes.DataStructure.Entity;
using Echoes.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Echoes.Data.Entity
{
    [Serializable]
    public class Persona : EntityPartial, IPersona
    {
        public IList<IAkashicEntry> AkashicRecord { get; set; }

        [JsonConstructor]
        public Persona() : base()
        {
            AkashicRecord = new List<IAkashicEntry>();
        }

        public Persona(StoredData storedData, StoredDataCache storedDataCache) : base(storedData, storedDataCache)
        {
            AkashicRecord = new List<IAkashicEntry>();
        }

        /// <summary>
        /// Method by which this entity has output (from commands and events) "shown" to it
        /// </summary>
        public override IEnumerable<IContext> WriteTo(string input, IPersona originator)
        {
            var newContext = base.WriteTo(input, originator);

            Observe(input, originator, newContext);

            Save();

            return newContext;
        }

        public void Observe(string observance, IPersona actor, IEnumerable<IContext> newContext)
        {
            AkashicRecord.Add(new AkashicEntry(DateTime.Now, observance, actor, newContext, DataCache));
        }

        public override IEnumerable<string> RenderToLocation()
        {
            var sb = new List<string>
            {
                string.Format("{0} is here", Name)
            };

            return sb;
        }

        public override IEnumerable<string> RenderToLook()
        {
            var sb = new List<string>
            {
                string.Format("<s>{0}</s>", Name)
            };

            return sb;
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
            DataCache.Add(this as IPersona);
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
