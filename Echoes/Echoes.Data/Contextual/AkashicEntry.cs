using Cottontail.Cache;
using Echoes.DataStructure.Contextual;
using Echoes.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Echoes.Data.Contextual
{
    [Serializable]
    public class AkashicEntry : IAkashicEntry
    {
        private StoredDataCache dataCache;

        public DateTime Timestamp { get; private set; }

        public string Observance { get; private set; }

        [JsonProperty("Actor")]
        private long _actor { get; set; }

        [JsonIgnore]
        public IEntity Actor
        {
            get
            {
                return dataCache.Get<IEntity>(_actor);
            }
            private set
            {
                _actor = value.ID;
            }
        }

        public IEnumerable<IContext> Context { get; private set; }

        public AkashicEntry(DateTime timestamp, string observance, IEntity actor, IEnumerable<IContext> context, string baseDirectory)
        {
            Timestamp = timestamp;
            Observance = observance;
            Actor = actor;
            Context = context;

            dataCache = new StoredDataCache(baseDirectory);
        }
    }
}
