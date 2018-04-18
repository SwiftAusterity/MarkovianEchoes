using Echoes.DataStructure.Contextual;
using Echoes.DataStructure.System;
using System;

namespace Echoes.Data.Contextual
{
    public class AkashicEntry : IAkashicEntry
    {
        public DateTime Timestamp { get; private set; }

        public string Observance { get; private set; }

        public IEntity Actor { get; private set; }

        public IContext Context { get; private set; }

        public AkashicEntry(DateTime timestamp, string observance, IEntity actor, IContext context)
        {
            Timestamp = timestamp;
            Observance = observance;
            Actor = actor;
            Context = context;
        }
    }
}
