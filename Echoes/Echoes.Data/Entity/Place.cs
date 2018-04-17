using Echoes.Data.System;
using Echoes.DataStructure.Entity;
using Echoes.DataStructure.System;
using System;
using System.Collections.Generic;

namespace Echoes.Data.Entity
{
    [Serializable]
    public class Place : EntityPartial, IPlace
    {
        public IEntityContainer Contents { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override IEnumerable<string> RenderToLook(IEntity actor)
        {
            throw new NotImplementedException();
        }

        public override void SpawnNewInWorld()
        {
            throw new NotImplementedException();
        }

        public override void SpawnNewInWorld(IContains spawnTo)
        {
            throw new NotImplementedException();
        }
    }
}
