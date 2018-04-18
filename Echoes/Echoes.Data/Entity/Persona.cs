﻿using Echoes.Data.Contextual;
using Echoes.Data.System;
using Echoes.DataStructure.Contextual;
using Echoes.DataStructure.Entity;
using Echoes.DataStructure.System;
using System;
using System.Collections.Generic;

namespace Echoes.Data.Entity
{
    [Serializable]
    public class Persona : EntityPartial, IPersona
    {
        public IList<IAkashicEntry> AkashicRecord { get; set; }

        /// <summary>
        /// Method by which this entity has output (from commands and events) "shown" to it
        /// </summary>
        public override bool WriteTo(string input, IEntity originator)
        {
            Observe(input, originator);

            return true;
        }

        public void Observe(string observance, IEntity actor)
        {
            var newContext = new Context();
            //= Echoes.MarkovianEngine.Experience(this, actor, observance);

            //Echoes.MarkovianEngine.Merge(FullContext, newContext);

            AkashicRecord.Add(new AkashicEntry(DateTime.Now, observance, actor, newContext));
        }

        public override IEnumerable<string> RenderToLocation()
        {
            var sb = new List<string>();

            sb.Add(string.Format("{0} is here", Name));

            return sb;
        }

        public override IEnumerable<string> RenderToLook()
        {
            var sb = new List<string>();

            sb.Add(string.Format("<s>{0}</s>", Name));

            return sb;
        }
    }
}
