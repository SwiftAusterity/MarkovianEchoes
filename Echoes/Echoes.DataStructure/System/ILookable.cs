﻿using System.Collections.Generic;

namespace Echoes.DataStructure.System
{
    /// <summary>
    /// Framework for rendering output when this entity is Look(ed) at
    /// </summary>
    public interface ILookable
    {
        /// <summary>
        /// Renders output for this entity when Look targets it
        /// </summary>
        /// <returns>the output</returns>
        IEnumerable<string> RenderToLook();
    }
}
