using System.Collections.Generic;

namespace Echoes.DataStructure.System
{
    /// <summary>
    /// Rendering methods for when a location that contains the entity being rendered is being rendered
    /// </summary>
    public interface IRenderInLocation
    {
        /// <summary>
        /// Renders output for this entity when Look targets it
        /// </summary>
        /// <returns>the output</returns>
        IEnumerable<string> RenderToLocation();
    }
}
