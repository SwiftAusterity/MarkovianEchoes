using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echoes.DataStructure.System
{
    /// <summary>
    /// Object that handles any and all "this contains entities" for the system
    /// </summary>
    /// <typeparam name="T">the type of entities it can contain</typeparam>
    public interface IEntityContainer
    {
        /// <summary>
        /// List of entities contained (it needs to never store its own objects, only cache references)
        /// </summary>
        IEnumerable<IEntity> EntitiesContained();

        /// <summary>
        /// Add an entity to this
        /// </summary>
        /// <param name="entity">the entity to add</param>
        /// <returns>success status</returns>
        bool Add(IEntity entity);

        /// <summary>
        /// Does this contain the specified entity
        /// </summary>
        /// <param name="entity">the entity in question</param>
        /// <returns>yes it contains it or no it does not</returns>
        bool Contains(IEntity entity);

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="entity">the entity to remove</param>
        /// <returns>success status</returns>
        bool Remove(IEntity entity);

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="birthMark">the entity's birthmark to remove</param>
        /// <returns>success status</returns>
        bool Remove(long ID);

        /// <summary>
        /// Count the entities in this
        /// </summary>
        /// <returns>the count</returns>
        int Count();
    }
}
