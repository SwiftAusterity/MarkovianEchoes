using Echoes.DataStructure.Contextual;
using System;

namespace Echoes.Data.Contextual
{
    /// <summary>
    /// Placeholder context for parsing
    /// </summary>
    [Serializable]
    public class Noun : INoun
    {
        public string Name { get; set; }
    }
}
