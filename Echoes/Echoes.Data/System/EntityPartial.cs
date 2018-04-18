using Cottontail.Cache;
using Cottontail.FileSystem;
using Cottontail.FileSystem.Logging;
using Cottontail.Structure;
using Cottontail.Utility.Data;
using Echoes.DataStructure.Contextual;
using Echoes.DataStructure.Entity;
using Echoes.DataStructure.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Echoes.Data.System
{
    /// <summary>
    /// Abstract that tries to keep the entity classes cleaner
    /// </summary>
    [Serializable]
    public abstract class EntityPartial : SerializableDataPartial, IEntity
    {
        #region Live tracking properties
        /// <summary>
        /// When this entity was born to the world
        /// </summary>
        public DateTime Birthdate { get; set; }
        #endregion

        /// <summary>
        /// Method by which this entity has output (from commands and events) "shown" to it
        /// </summary>
        public virtual bool WriteTo(string input, IEntity originator)
        {
            return TriggerAIAction(input);
        }

        public IList<IContext> FullContext { get; set; }
        public long ID { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastRevised { get; set; }
        public string Name { get; set; }
        public IContains Position { get; set; }

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public virtual void SpawnNewInWorld()
        {
            SpawnNewInWorld(GetBaseSpawn());
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public virtual void SpawnNewInWorld(IContains spawnTo)
        {
            spawnTo.MoveInto(this);
            UpsertToLiveWorldCache();
        }

        /// <summary>
        /// Move this inside of something
        /// </summary>
        /// <param name="container">The container to move into</param>
        /// <returns>was this thing moved?</returns>
        public virtual bool TryMoveInto(IContains container)
        {
            return container.MoveInto(this);
        }
        
        /// <summary>
        /// Update this entry to the live world cache
        /// </summary>
        public void UpsertToLiveWorldCache()
        {
            StoredDataCache.Add(this);
        }

        /// <summary>
        /// For non-player entities - accepts output "shown" to it by the parser as a result of commands and events
        /// </summary>
        /// <param name="input">the output strings</param>
        /// <param name="trigger">the methodology type (heard, seen, etc)</param>
        /// <returns></returns>
        public bool TriggerAIAction(string input, AITriggerType trigger = AITriggerType.Seen)
        {
            //TODO: Actual AI code
            return true;
        }

        /// <summary>
        /// Render this to a look command in a container this is in
        /// </summary>
        /// <returns>the output strings</returns>
        public abstract IEnumerable<string> RenderToLocation();

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <returns>the output strings</returns>
        public abstract IEnumerable<string> RenderToLook();

        public bool ConveyMeaning(IContext context)
        {
            //return Echoes.MarkovianEngine.Merge(FullContext, context);

            return true;
        }

        public bool ConveyMeaning(IEnumerable<IContext> context)
        {
            //return Echoes.MarkovianEngine.Merge(FullContext, context);

            return true;
        }

        /// <summary>
        /// Find the emergency we dont know where to spawn this guy spawn location
        /// </summary>
        /// <returns>The emergency spawn location</returns>
        internal IContains GetBaseSpawn()
        {
            return StoredDataCache.GetAll<IPlace>().FirstOrDefault();
        }

        #region Data Functions
        /// <summary>
        /// Add it to the cache and save it to the file system
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public virtual IData Create()
        {
            var accessor = new StoredData();

            try
            {
                //reset this guy's ID to the next one in the list
                GetNextId();

                StoredDataCache.Add(this);
                accessor.WriteEntity(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                return null;
            }

            return this;
        }

        /// <summary>
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool Remove()
        {
            var accessor = new StoredData();

            try
            {
                //Remove from cache first
                StoredDataCache.Remove(new BackingDataCacheKey(this.GetType(), this.ID));

                //Remove it from the file system.
                accessor.ArchiveEntity(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool Save()
        {
            var accessor = new StoredData();

            try
            {
                accessor.WriteEntity(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Grabs the next ID in the chain of all objects of this type.
        /// </summary>
        internal void GetNextId()
        {
            IEnumerable<IData> allOfMe = StoredDataCache.GetAll().Where(bdc => bdc.GetType() == this.GetType());

            //Zero ordered list
            if (allOfMe.Count() > 0)
                ID = allOfMe.Max(dp => dp.ID) + 1;
            else
                ID = 0;
        }
        #endregion

        #region Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(IEntity other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != this.GetType())
                        return -1;

                    if (other.ID.Equals(this.ID))
                        return 1;

                    return 0;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(IEntity other)
        {
            if (other != default(IEntity))
            {
                try
                {
                    return other.GetType() == this.GetType() && other.ID.Equals(this.ID);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

        public int CompareTo(IData other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != this.GetType())
                        return -1;

                    if (other.ID.Equals(this.ID))
                        return 1;

                    return 0;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        public bool Equals(IData other)
        {
            if (other != default(IData))
            {
                try
                {
                    return other.GetType() == this.GetType() && other.ID.Equals(this.ID);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }
        #endregion
    }
}
