using Echoes.DataStructure.Contextual;
using System;

namespace Echoes.Data.Contextual
{
    [Serializable]
    public class Descriptor : IDescriptor
    {
        public bool Applied { get; set; }
        public string Opposite { get; set; }
        public string Name { get; set; }

        public Descriptor()
        {
            Applied = false;
        }
    }
}
