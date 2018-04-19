using Echoes.Data.System;
using Echoes.DataStructure.Entity;
using System;
using System.Collections.Generic;

namespace Echoes.Data.Entity
{
    [Serializable]
    public class Thing : EntityPartial, IThing
    {
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
    }
}
