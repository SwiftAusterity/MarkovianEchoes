using Cottontail.Cache;
using Echoes.Data.Entity;
using Echoes.DataStructure.Contextual;
using Echoes.DataStructure.Entity;
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
        public IPersona Actor
        {
            get
            {
                return dataCache.Get<Persona>(_actor);
            }
            private set
            {
                _actor = value.ID;
            }
        }

        public IEnumerable<IContext> Context { get; private set; }

        [JsonConstructor]
        public AkashicEntry(DateTime timestamp, string observance, long actor, IEnumerable<IContext> context, StoredDataCache storedDataCache)
        {
            Timestamp = timestamp;
            Observance = observance;
            _actor = actor;
            Context = context;

            dataCache = storedDataCache;
        }

        public AkashicEntry(DateTime timestamp, string observance, IPersona actor, IEnumerable<IContext> context, StoredDataCache storedDataCache)
        {
            Timestamp = timestamp;
            Observance = observance;
            Actor = actor;
            Context = context;

            dataCache = storedDataCache;
        }
    }
}
