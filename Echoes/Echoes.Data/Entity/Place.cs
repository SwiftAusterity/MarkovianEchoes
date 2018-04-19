using Cottontail.Structure;
using Echoes.Data.System;
using Echoes.DataStructure.Entity;
using Echoes.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Echoes.Data.Entity
{
    [Serializable]
    public class Place : EntityPartial, IPlace
    {
        #region Container
        [JsonIgnore]
        public ICacheContainer<IEntity> Inventory { get; set; }

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
    }
}
