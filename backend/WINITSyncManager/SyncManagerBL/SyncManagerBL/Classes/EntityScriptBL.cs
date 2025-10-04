using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerBL.Classes
{
    public class EntityScriptBL : IEntityScriptBL
    {
        private readonly IEntityScriptDL _entityScript;
        public EntityScriptBL(IEntityScriptDL entityScriptDL)
        {
            _entityScript = entityScriptDL;
        }
        public async Task<IEntityScript> GetEntityScriptDetailsByEntity(string Entity)
        {
            return await _entityScript.GetEntityScriptDetailsByEntity(Entity);
        }
    }
}
