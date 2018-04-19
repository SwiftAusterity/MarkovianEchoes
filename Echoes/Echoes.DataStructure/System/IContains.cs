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
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <returns>errors</returns>
        bool MoveInto(IEntity thing);

        /// <summary>
        /// Move an entity out of this
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        bool MoveFrom(IEntity thing);

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        IEnumerable<IEntity> GetContents();
    }
}
