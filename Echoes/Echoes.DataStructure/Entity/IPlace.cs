using Echoes.DataStructure.System;

namespace Echoes.DataStructure.Entity
{
    public interface IPlace : IEntity, ILookable
    {
        /// <summary>
        /// Mobiles (NPC, Players) in the room
        /// </summary>
        IEntityContainer<IEntity> Contents { get; set; }
    }
}
