using Echoes.DataStructure.Contextual;
using System;

namespace Echoes.Data.Contextual
{
    [Serializable]
    public class Context : IContext
    {
        public string Name { get; set; }
    }
}
