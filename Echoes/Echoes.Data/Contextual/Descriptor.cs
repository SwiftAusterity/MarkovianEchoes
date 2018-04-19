using Echoes.DataStructure.Contextual;
using System;

namespace Echoes.Data.Contextual
{
    [Serializable]
    public class Descriptor : IDescriptor
    {
        public string Opposite { get; set; }
        public string Name { get; set; }
    }
}
