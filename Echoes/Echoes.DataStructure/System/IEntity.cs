using Cottontail.Cache;
using Cottontail.FileSystem;
using Cottontail.FileSystem.Logging;
using Cottontail.Structure;
using Echoes.DataStructure.Contextual;
using Echoes.DataStructure.Entity;
using System;
using System.Collections.Generic;

namespace Echoes.DataStructure.System
{
    /// <summary>
    /// The basis for anything that shows up in game
    /// </summary>
    public interface IEntity : IData, IRenderInLocation, ILookable, IComparable<IEntity>, IEquatable<IEntity>
    {
        /// <summary>
        /// List of descriptors this objects knows
        /// </summary>
        IEnumerable<IContext> FullContext { get; set; }

        /// <summary>
        /// Where this is in the world
        /// </summary>
        IContains Position { get; set;  }

        /// <summary>
        /// Transfer context from one entity to this one
        /// </summary>
        /// <param name="context">the context being transferred</param>
        /// <returns>success</returns>
        bool ConveyMeaning(IContext context);

        /// <summary>
        /// Transfer context from one entity to this one
        /// </summary>
        /// <param name="context">the list of contexts being transferred</param>
        /// <returns>success</returns>
        bool ConveyMeaning(IEnumerable<IContext> context);

        /// <summary>
        /// Method by which this entity has output (from commands and events) "shown" to it
        /// </summary>
        IEnumerable<IContext> WriteTo(string input, IPersona originator, bool acting);

        /// <summary>
        /// Update this to the live cache
        /// </summary>
        void UpsertToLiveWorldCache();

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        void SpawnNewInWorld();

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        void SpawnNewInWorld(IContains spawnTo);

        void SetAccessors(StoredDataFileAccessor storedData, StoredDataCache storedDataCache, FileLogger logger);
    }
}
