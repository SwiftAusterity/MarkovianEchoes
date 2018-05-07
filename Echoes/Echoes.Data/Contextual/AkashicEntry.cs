using Cottontail.Cache;
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

        public DateTime Timestamp { get; set; }

        public string Observance { get; set; }

        public bool Spoken { get; set; }

        [JsonProperty("Actor")]
        private long _actor { get; set; }

        [JsonIgnore]
        public IPersona Actor
        {
            get
            {
                return dataCache.Get<IPersona>(_actor);
            }
            set
            {
                _actor = value.ID;
            }
        }

        public IEnumerable<IContext> Context { get; set; }

        public AkashicEntry()
        {
            Context = new List<IContext>();
        }

        [JsonConstructor]
        public AkashicEntry(DateTime timestamp, string observance, bool spoken, long actor, IEnumerable<IContext> context, StoredDataCache storedDataCache)
        {
            Timestamp = timestamp;
            Observance = observance;
            _actor = actor;
            Context = context;
            Spoken = spoken;

            dataCache = storedDataCache;
        }

        public AkashicEntry(DateTime timestamp, string observance, bool spoken, IPersona actor, IEnumerable<IContext> context, StoredDataCache storedDataCache)
        {
            Timestamp = timestamp;
            Observance = observance;
            Actor = actor;
            Context = context;
            Spoken = spoken;

            dataCache = storedDataCache;
        }

        public void SetAccessors(StoredDataCache storedDataCache)
        {
            dataCache = storedDataCache;
        }
    }
}
