using Cottontail.Cache;
using Cottontail.FileSystem.Logging;
using Echoes.Data.System;
using Echoes.DataStructure.Entity;
using System;
using System.Collections.Generic;

namespace Echoes.Data.Entity
{
    [Serializable]
    public class Thing : EntityPartial, IThing
    {
        public Thing(string baseDirectory) : base(baseDirectory) { }

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
