using Echoes.DataStructure.System;
using System.Collections.Generic;
using System.Linq;

namespace Echoes.Web.Models
{
    public class EntityInfoModel
    {
        public string Errors { get; set; }

        public IEntity Entity { get; set; }
        public IEnumerable<string> Things { get; set; }
        public IEnumerable<string> Personas { get; set; }
        public IEnumerable<string> Adjectives { get; set; }
        public IEnumerable<string> Actions { get; set; }

        public EntityInfoModel()
        {
            Things = Enumerable.Empty<string>();
            Personas = Enumerable.Empty<string>();
            Adjectives = Enumerable.Empty<string>();
            Actions = Enumerable.Empty<string>();
        }
    }
}
