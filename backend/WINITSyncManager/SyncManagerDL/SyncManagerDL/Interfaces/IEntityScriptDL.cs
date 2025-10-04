using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerDL.Interfaces
{
    public interface IEntityScriptDL
    {
        Task<SyncManagerModel.Interfaces.IEntityScript> GetEntityScriptDetailsByEntity(string Entity);
    }
}
