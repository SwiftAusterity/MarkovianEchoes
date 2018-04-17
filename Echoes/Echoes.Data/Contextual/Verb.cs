using Echoes.DataStructure.Contextual;
using System;
using System.Collections.Generic;

namespace Echoes.Data.Contextual
{
    [Serializable]
    public class Verb : IVerb
    {
        public Dictionary<string, Tuple<ActionType, string>> Affects { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name { get; set; }
    }
}
