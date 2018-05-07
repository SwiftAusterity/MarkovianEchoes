using Cottontail.Structure;
using Echoes.DataStructure.System;
using System.Collections.Generic;

namespace Echoes.DataStructure.Entity
{
    public interface IPlace : ILookable, IContains
    {
        /// <summary>
        /// Players in the place
        /// </summary>
        ICacheContainer<IPersona> PersonaInventory { get; set; }

        /// <summary>
        /// Things in the place
        /// </summary>
        ICacheContainer<IThing> ThingInventory { get; set; }

        /// <summary>
        /// Places linked from here
        /// </summary>
        IList<IPlace> LinkedPlaces { get; set; }

        /// <summary>
        /// Add a place to the linked list
        /// </summary>
        /// <param name="place"></param>
        void LinkPlace(IPlace place);
    }
}
