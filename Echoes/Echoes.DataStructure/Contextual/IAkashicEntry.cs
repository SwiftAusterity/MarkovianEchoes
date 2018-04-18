﻿using Echoes.DataStructure.System;
using System;

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
        DateTime Timestamp { get; }

        /// <summary>
        /// The raw input that spawned this record
        /// </summary>
        string Observance { get; }

        /// <summary>
        /// The originator of the occurance
        /// </summary>
        IEntity Actor { get; }

        /// <summary>
        /// The context generated for the observer from the occurance
        /// </summary>
        IContext Context { get; }
    }
}
