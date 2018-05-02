using Echoes.DataStructure.Entity;
using System;
using System.Collections.Generic;

namespace Echoes.DataStructure.Contextual
{
    /// <summary>
    /// Single entry in a Persona's Akashic Record
    /// </summary>
    public interface IAkashicEntry
    {
        /// <summary>
        /// The time this occured
        /// </summary>
        DateTime Timestamp { get; set; }

        /// <summary>
        /// The raw input that spawned this record
        /// </summary>
        string Observance { get; set; }

        /// <summary>
        /// Whether or not the observance was spoken or acted
        /// </summary>
        bool Spoken { get; set; }

        /// <summary>
        /// The originator of the occurance
        /// </summary>
        IPersona Actor { get; set; }

        /// <summary>
        /// The context generated for the observer from the occurance
        /// </summary>
        IEnumerable<IContext> Context { get; set; }
    }
}
