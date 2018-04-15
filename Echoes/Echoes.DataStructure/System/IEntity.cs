using Cottontail.Structure;
using Echoes.DataStructure.Contextual;
using System;
using System.Collections.Generic;

namespace Echoes.DataStructure.System
{
    /// <summary>
    /// The basis for anything that shows up in game
    /// </summary>
    public interface IEntity : IData, IComparable<IEntity>, IEquatable<IEntity>
    {
        /// <summary>
        /// is this an AI or not
        /// </summary>
        bool IsAI { get; }

        /// <summary>
        /// List of descriptors this objects knows
        /// </summary>
        IEnumerable<IContext> FullContext { get; set; }

        /// <summary>
        /// Transfer context from one entity to this one
        /// </summary>
        /// <param name="context">the context being transferred</param>
        /// <returns>success</returns>
        bool ConveyMeaning(IContext context);

        /// <summary>
        /// Transfer context from one entity to this one
        /// </summary>
        /// <param name="context">the list of contexts being transferred</param>
        /// <returns>success</returns>
        bool ConveyMeaning(IEnumerable<IContext> context);

        /// <summary>
        /// For non-player entities - accepts output "shown" to it by the parser as a result of commands and events
        /// </summary>
        /// <param name="input">the output strings</param>
        /// <param name="trigger">the methodology type (heard, seen, etc)</param>
        /// <returns></returns>
        bool TriggerAIAction(IEnumerable<string> input, AITriggerType trigger = AITriggerType.Seen);

        /// <summary>
        /// Method by which this entity has output (from commands and events) "shown" to it
        /// </summary>
        bool WriteTo(IEnumerable<string> input);

        /// <summary>
        /// Update this to the live cache
        /// </summary>
        void UpsertToLiveWorldCache();
    }
}
