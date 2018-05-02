using Echoes.DataStructure.Contextual;
using Echoes.DataStructure.Entity;
using System.Collections.Generic;

namespace Echoes.DataStructure.System
{
    /// <summary>
    /// Handles observation of input
    /// </summary>
    public interface IObserve
    {
        /// <summary>
        /// The full list of akashic records in order
        /// </summary>
        IList<IAkashicEntry> AkashicRecord { get; set; }

        /// <summary>
        /// Invoked when input happens in the same place as this
        /// </summary>
        /// <param name="observance">The raw input</param>
        /// <param name="actor">Who originated it</param>
        void Observe(string observance, IPersona actor, IEnumerable<IContext> newContext, bool spoken);
    }
}
