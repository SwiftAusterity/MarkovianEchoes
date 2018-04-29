using Echoes.DataStructure.Entity;
using System.Collections.Generic;

namespace Echoes.DataStructure.System
{
    /// <summary>
    /// Framework for entities that can hold other entities
    /// </summary>
    public interface IContains : IEntity
    {
        /// <summary>
        /// Move an entity into this
        /// </summary>
        /// <param name="thing">the entity to add</param>
        /// <returns>errors</returns>
        bool MoveInto(IPersona thing);

        /// <summary>
        /// Move an entity out of this
        /// </summary>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        bool MoveFrom(IPersona thing);

        /// <summary>
        /// Move an entity into this
        /// </summary>
        /// <param name="thing">the entity to add</param>
        /// <returns>errors</returns>
        bool MoveInto(IThing thing);

        /// <summary>
        /// Move an entity out of this
        /// </summary>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        bool MoveFrom(IThing thing);

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <returns>the contained entities</returns>
        IEnumerable<IPersona> GetPersonas();

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <returns>the contained entities</returns>
        IEnumerable<IThing> GetThings();
    }
}
