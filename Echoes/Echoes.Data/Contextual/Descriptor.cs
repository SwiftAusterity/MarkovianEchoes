using Echoes.DataStructure.Contextual;
using System;

namespace Echoes.Data.Contextual
{
    [Serializable]
    public class Descriptor : IDescriptor
    {
        public string Opposite { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name { get; set; }
    }
}
