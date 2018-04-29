using Cottontail.Structure;
using Echoes.DataStructure.System;

namespace Echoes.DataStructure.Entity
{
    public interface IPlace : ILookable, IContains
    {
        /// <summary>
        /// Mobiles (NPC, Players) in the room
        /// </summary>
        ICacheContainer<IPersona> PersonaInventory { get; set; }

        ICacheContainer<IThing> ThingInventory { get; set; }
    }
}
