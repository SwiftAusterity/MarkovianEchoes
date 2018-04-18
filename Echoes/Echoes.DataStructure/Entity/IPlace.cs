using Cottontail.Structure;
using Echoes.DataStructure.System;

namespace Echoes.DataStructure.Entity
{
    public interface IPlace : IEntity, ILookable, IContains
    {
        /// <summary>
        /// Mobiles (NPC, Players) in the room
        /// </summary>
        ICacheContainer<IEntity> Inventory { get; set; }
    }
}
