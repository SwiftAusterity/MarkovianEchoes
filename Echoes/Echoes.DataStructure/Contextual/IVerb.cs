using System;
using System.Collections.Generic;

namespace Echoes.DataStructure.Contextual
{
    /// <summary>
    /// Actions that can be done within a context
    /// </summary>
    public interface IVerb : IContext
    {
        /// <summary>
        /// What affects this has on the current context. Key: Context target, Value: What do to, Context (to apply or remove)
        /// </summary>
        Dictionary<string, Tuple<ActionType, string>> Affects { get; set; }
    }

    public enum ActionType
    {
        Apply,
        Remove
    }
}
