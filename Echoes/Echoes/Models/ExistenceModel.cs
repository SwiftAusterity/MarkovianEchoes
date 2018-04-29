using Echoes.DataStructure.Contextual;
using Echoes.DataStructure.Entity;
using System.Collections.Generic;

namespace Echoes.Web.Models
{
    public class ExistenceModel
    {
        public string Errors { get; set; }

        public string Input { get; set; }

        public IPersona CurrentPersona { get; set; }
        public IPlace CurrentPlace { get; set; }
        public IEnumerable<IAkashicEntry> NewToYou { get; set; }

        public ExistenceModel()
        {
            NewToYou = new List<IAkashicEntry>();
        }
    }
}
