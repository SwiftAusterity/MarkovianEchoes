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
using System.Linq;

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

        public Persona(StoredDataFileAccessor storedData, StoredDataCache storedDataCache, FileLogger logger) : base(storedData, storedDataCache, logger)
        {
            AkashicRecord = new List<IAkashicEntry>();
        }

        public override void SetAccessors(StoredDataFileAccessor storedData, StoredDataCache storedDataCache, FileLogger logger)
        {
            foreach(var entry in AkashicRecord)
            {
                entry.SetAccessors(storedDataCache);
            }

            base.SetAccessors(storedData, storedDataCache, logger);
        }

        /// <summary>
        /// Method by which this entity has output (from commands and events) "shown" to it
        /// </summary>
        public override IEnumerable<IContext> WriteTo(string input, IPersona originator, bool acting)
        {
            var newContext = base.WriteTo(input, originator, acting);

            Observe(input, originator, newContext, !acting);

            Save();

            return newContext;
        }

        public void Observe(string observance, IPersona actor, IEnumerable<IContext> newContext, bool spoken)
        {
            AkashicRecord.Add(new AkashicEntry(DateTime.Now, observance, spoken, actor, newContext, DataCache));
        }

        public override IEnumerable<string> RenderToLocation()
        {
            var decorators = FullContext.Where(adj => adj.GetType() == typeof(Descriptor) && ((Descriptor)adj).Applied).Select(desc => desc.Name);
            var sb = new List<string>();

            if (decorators.Count() > 0)         
                sb.Add(string.Format("{0} is standing here appearing quite {1}.", Name, String.Join(",", decorators)));
            else
                sb.Add(string.Format("{0} is standing here.", Name));

            return sb;
        }

        public override IEnumerable<string> RenderToLook()
        {
            var sb = new List<string>
            {
                string.Format("{0}", Name)
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
                Logger.LogError(ex);
                return false;
            }

            return true;
        }
    }
}
