﻿using Echoes.DataStructure.Contextual;
using System;
using System.Collections.Generic;

namespace Echoes.Data.Contextual
{
    /// <summary>
    /// Actions that can be done within a context
    /// </summary>
    [Serializable]
    public class Verb : IVerb
    {
        /// <summary>
        /// What affects this has on the current context. Key: Context target, Value: What do to, Context (to apply or remove)
        /// </summary>
        public Dictionary<string, Tuple<ActionType, string>> Affects { get; set; }

        public string Name { get; set; }

        public Verb()
        {
            Affects = new Dictionary<string, Tuple<ActionType, string>>();
        }
    }
}
